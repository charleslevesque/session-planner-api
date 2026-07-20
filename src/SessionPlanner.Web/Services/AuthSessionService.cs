using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SessionPlanner.Web.Models;

namespace SessionPlanner.Web.Services;

public sealed class AuthSessionService(HttpClient httpClient, SessionStorageService storage)
{
    private const string SessionKey = "sp_auth_session";
    private const string UserKey = "sp_auth_user";

    private bool _initialized;
    private CancellationTokenSource? _refreshCts;

    public event Action? StateChanged;

    public AuthResponse? Session { get; private set; }
    public MeResponse? User { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Session?.Token) && User is not null;
    public string? AccessToken => Session?.Token;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        var sessionJson = await storage.GetAsync(SessionKey);
        var userJson = await storage.GetAsync(UserKey);

        if (string.IsNullOrWhiteSpace(sessionJson) || string.IsNullOrWhiteSpace(userJson))
        {
            await ClearAsync(notify: true);
            return;
        }

        try
        {
            Session = JsonSerializer.Deserialize<AuthResponse>(sessionJson, JsonDefaults.Options);
            User = JsonSerializer.Deserialize<MeResponse>(userJson, JsonDefaults.Options);
        }
        catch
        {
            await ClearAsync(notify: true);
            return;
        }

        if (Session is null || User is null)
        {
            await ClearAsync(notify: true);
            return;
        }

        if (Session.ExpiresAt <= DateTime.UtcNow.AddSeconds(30))
        {
            if (!await RefreshAsync())
            {
                return;
            }
        }

        try
        {
            User = await SendRawAsync<MeResponse>(HttpMethod.Get, "auth/me", token: Session.Token);
            await storage.SetAsync(UserKey, JsonSerializer.Serialize(User, JsonDefaults.Options));
            ScheduleRefresh();
            NotifyStateChanged();
        }
        catch
        {
            await ClearAsync(notify: true);
        }
    }

    public async Task LoginAsync(LoginRequest request)
    {
        var nextSession = await SendRawAsync<AuthResponse>(HttpMethod.Post, "auth/login", request);
        var profile = await SendRawAsync<MeResponse>(HttpMethod.Get, "auth/me", token: nextSession.Token);
        await ApplySessionAsync(nextSession, profile);
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        await SendRawAsync<AuthResponse>(HttpMethod.Post, "auth/register", request);
        await ClearAsync(notify: true);
    }

    public async Task LogoutAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(Session?.Token) && !string.IsNullOrWhiteSpace(Session.RefreshToken))
            {
                await SendRawAsync<object?>(HttpMethod.Post, "auth/logout",
                    new RefreshTokenRequest { RefreshToken = Session.RefreshToken },
                    Session.Token);
            }
        }
        finally
        {
            await ClearAsync(notify: true);
        }
    }

    public async Task<bool> RefreshAsync()
    {
        if (string.IsNullOrWhiteSpace(Session?.RefreshToken))
        {
            await ClearAsync(notify: true);
            return false;
        }

        try
        {
            var nextSession = await SendRawAsync<AuthResponse>(HttpMethod.Post, "auth/refresh",
                new RefreshTokenRequest { RefreshToken = Session.RefreshToken });
            var profile = await SendRawAsync<MeResponse>(HttpMethod.Get, "auth/me", token: nextSession.Token);
            await ApplySessionAsync(nextSession, profile);
            return true;
        }
        catch
        {
            await ClearAsync(notify: true);
            return false;
        }
    }

    public async Task<bool> RefreshCurrentUserAsync()
    {
        await InitializeAsync();

        if (string.IsNullOrWhiteSpace(Session?.Token))
        {
            await ClearAsync(notify: true);
            return false;
        }

        try
        {
            User = await SendRawAsync<MeResponse>(HttpMethod.Get, "auth/me", token: Session.Token);
            await storage.SetAsync(UserKey, JsonSerializer.Serialize(User, JsonDefaults.Options));
            NotifyStateChanged();
            return true;
        }
        catch
        {
            await ClearAsync(notify: true);
            return false;
        }
    }

    public async Task<string?> GetFreshAccessTokenAsync()
    {
        await InitializeAsync();

        if (Session is null)
        {
            return null;
        }

        if (Session.ExpiresAt <= DateTime.UtcNow.AddMinutes(1))
        {
            await RefreshAsync();
        }

        return Session?.Token;
    }

    private async Task ApplySessionAsync(AuthResponse nextSession, MeResponse nextUser)
    {
        Session = nextSession;
        User = nextUser;
        await storage.SetAsync(SessionKey, JsonSerializer.Serialize(nextSession, JsonDefaults.Options));
        await storage.SetAsync(UserKey, JsonSerializer.Serialize(nextUser, JsonDefaults.Options));
        ScheduleRefresh();
        NotifyStateChanged();
    }

    private async Task ClearAsync(bool notify)
    {
        _refreshCts?.Cancel();
        Session = null;
        User = null;
        await storage.RemoveAsync(SessionKey);
        await storage.RemoveAsync(UserKey);

        if (notify)
        {
            NotifyStateChanged();
        }
    }

    private void ScheduleRefresh()
    {
        _refreshCts?.Cancel();

        if (Session is null)
        {
            return;
        }

        var delay = Session.ExpiresAt - DateTime.UtcNow - TimeSpan.FromMinutes(1);
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        var cts = new CancellationTokenSource();
        _refreshCts = cts;
        _ = RefreshAfterDelayAsync(delay, cts.Token);
    }

    private async Task RefreshAfterDelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
            {
                await RefreshAsync();
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    private async Task<T> SendRawAsync<T>(HttpMethod method, string path, object? body = null, string? token = null)
    {
        using var request = new HttpRequestMessage(method, path.TrimStart('/'));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonDefaults.Options),
                Encoding.UTF8,
                "application/json");
        }

        using var response = await httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default!;
        }

        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(GetErrorMessage(responseText, response.ReasonPhrase), (int)response.StatusCode, responseText);
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)responseText;
        }

        if (string.IsNullOrWhiteSpace(responseText))
        {
            return default!;
        }

        return JsonSerializer.Deserialize<T>(responseText, JsonDefaults.Options)!;
    }

    private static string GetErrorMessage(string? responseText, string? fallback)
    {
        if (!string.IsNullOrWhiteSpace(responseText))
        {
            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(responseText, JsonDefaults.Options);
                if (!string.IsNullOrWhiteSpace(error?.Error))
                {
                    return error.Error;
                }
            }
            catch
            {
                return responseText;
            }
        }

        return fallback ?? "Une erreur est survenue.";
    }
}
