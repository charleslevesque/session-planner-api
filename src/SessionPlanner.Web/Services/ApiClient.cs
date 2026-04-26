using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SessionPlanner.Web.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public ApiClient(HttpClient http) => _http = http;

    private static string Prefix(string path) =>
        path.StartsWith("/") ? $"/api/v1{path}" : $"/api/v1/{path}";

    private async Task EnsureSuccessAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;
        string msg;
        try
        {
            var err = await resp.Content.ReadFromJsonAsync<ErrorBody>(_opts);
            msg = err?.Error ?? resp.ReasonPhrase ?? resp.StatusCode.ToString();
        }
        catch
        {
            msg = resp.ReasonPhrase ?? resp.StatusCode.ToString();
        }
        throw new ApiException(msg, (int)resp.StatusCode);
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        var resp = await _http.GetAsync(Prefix(path));
        await EnsureSuccessAsync(resp);
        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default;
        return await resp.Content.ReadFromJsonAsync<T>(_opts);
    }

    public async Task<TRes?> PostAsync<TReq, TRes>(string path, TReq body)
    {
        var resp = await _http.PostAsJsonAsync(Prefix(path), body, _opts);
        await EnsureSuccessAsync(resp);
        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default;
        return await resp.Content.ReadFromJsonAsync<TRes>(_opts);
    }

    public async Task PostVoidAsync<TReq>(string path, TReq body)
    {
        var resp = await _http.PostAsJsonAsync(Prefix(path), body, _opts);
        await EnsureSuccessAsync(resp);
    }

    public async Task PostVoidAsync(string path)
    {
        var resp = await _http.PostAsync(Prefix(path), null);
        await EnsureSuccessAsync(resp);
    }

    public async Task<TRes?> PutAsync<TReq, TRes>(string path, TReq body)
    {
        var resp = await _http.PutAsJsonAsync(Prefix(path), body, _opts);
        await EnsureSuccessAsync(resp);
        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default;
        return await resp.Content.ReadFromJsonAsync<TRes>(_opts);
    }

    public async Task DeleteAsync(string path)
    {
        var resp = await _http.DeleteAsync(Prefix(path));
        await EnsureSuccessAsync(resp);
    }

    private record ErrorBody(string? Error);
}
