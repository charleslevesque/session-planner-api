using Blazored.SessionStorage;
using SessionPlanner.Web.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace SessionPlanner.Web.Services;

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";
    private const string RefreshKey = "auth_refresh_token";
    private const string ExpiresKey = "auth_expires_at";
    private const string UserKey = "auth_user";

    private readonly ISessionStorageService _storage;
    private readonly HttpClient _http;
    private Timer? _refreshTimer;

    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public event Action OnAuthStateChanged = () => { };

    public AuthService(ISessionStorageService storage, IHttpClientFactory factory)
    {
        _storage = storage;
        _http = factory.CreateClient("API");
    }

    public async Task<string?> GetTokenAsync() =>
        await _storage.GetItemAsync<string>(TokenKey);

    public async Task<MeResponse?> GetStoredUserAsync() =>
        await _storage.GetItemAsync<MeResponse>(UserKey);

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/api/v1/auth/login",
                new LoginRequest(email, password), _opts);
            if (!resp.IsSuccessStatusCode) return false;

            var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>(_opts);
            if (auth == null) return false;

            await StoreTokensAsync(auth);

            var me = await FetchMeAsync(auth.Token);
            if (me == null) { await ClearStorageAsync(); return false; }

            await _storage.SetItemAsync(UserKey, me);
            ScheduleRefresh(auth.ExpiresAt);
            OnAuthStateChanged();
            return true;
        }
        catch { return false; }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var refresh = await _storage.GetItemAsync<string>(RefreshKey);
            if (refresh != null)
                await _http.PostAsJsonAsync("/api/v1/auth/logout",
                    new { refreshToken = refresh }, _opts);
        }
        catch { }
        finally
        {
            _refreshTimer?.Dispose();
            await ClearStorageAsync();
            OnAuthStateChanged();
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest req)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/api/v1/auth/register", req, _opts);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refresh = await _storage.GetItemAsync<string>(RefreshKey);
            if (string.IsNullOrEmpty(refresh)) return false;

            var resp = await _http.PostAsJsonAsync("/api/v1/auth/refresh",
                new RefreshTokenRequest(refresh), _opts);
            if (!resp.IsSuccessStatusCode) { await ClearStorageAsync(); return false; }

            var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>(_opts);
            if (auth == null) { await ClearStorageAsync(); return false; }

            await StoreTokensAsync(auth);
            ScheduleRefresh(auth.ExpiresAt);
            return true;
        }
        catch { return false; }
    }

    public async Task RestoreSessionAsync()
    {
        var token = await _storage.GetItemAsync<string>(TokenKey);
        if (string.IsNullOrEmpty(token)) return;

        var me = await FetchMeAsync(token);
        if (me == null)
        {
            await ClearStorageAsync();
            return;
        }
        await _storage.SetItemAsync(UserKey, me);
        var expires = await _storage.GetItemAsync<string>(ExpiresKey);
        if (expires != null) ScheduleRefresh(expires);
        OnAuthStateChanged();
    }

    private async Task<MeResponse?> FetchMeAsync(string token)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode
                ? await resp.Content.ReadFromJsonAsync<MeResponse>(_opts)
                : null;
        }
        catch { return null; }
    }

    private async Task StoreTokensAsync(AuthResponse auth)
    {
        await _storage.SetItemAsync(TokenKey, auth.Token);
        await _storage.SetItemAsync(RefreshKey, auth.RefreshToken);
        await _storage.SetItemAsync(ExpiresKey, auth.ExpiresAt);
    }

    private async Task ClearStorageAsync()
    {
        await _storage.RemoveItemAsync(TokenKey);
        await _storage.RemoveItemAsync(RefreshKey);
        await _storage.RemoveItemAsync(ExpiresKey);
        await _storage.RemoveItemAsync(UserKey);
    }

    private void ScheduleRefresh(string expiresAt)
    {
        _refreshTimer?.Dispose();
        if (!DateTime.TryParse(expiresAt, out var exp)) return;
        var delay = exp.ToUniversalTime() - DateTime.UtcNow - TimeSpan.FromSeconds(60);
        if (delay <= TimeSpan.Zero) delay = TimeSpan.FromSeconds(5);
        _refreshTimer = new Timer(async _ => await RefreshTokenAsync(), null, delay, Timeout.InfiniteTimeSpan);
    }
}
