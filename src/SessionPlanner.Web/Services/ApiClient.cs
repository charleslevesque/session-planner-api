using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using SessionPlanner.Web.Models;

namespace SessionPlanner.Web.Services;

public sealed class ApiClient(HttpClient httpClient, AuthSessionService auth, IJSRuntime jsRuntime)
{
    public Task<T> GetAsync<T>(string path) =>
        SendAsync<T>(HttpMethod.Get, path);

    public Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body) =>
        SendAsync<TResponse>(HttpMethod.Post, path, body);

    public Task PostAsync<TRequest>(string path, TRequest body) =>
        SendAsync<object?>(HttpMethod.Post, path, body);

    public Task PostAsync(string path) =>
        SendAsync<object?>(HttpMethod.Post, path);

    public Task<TResponse> PutAsync<TRequest, TResponse>(string path, TRequest body) =>
        SendAsync<TResponse>(HttpMethod.Put, path, body);

    public Task PutAsync<TRequest>(string path, TRequest body) =>
        SendAsync<object?>(HttpMethod.Put, path, body);

    public Task DeleteAsync(string path) =>
        SendAsync<object?>(HttpMethod.Delete, path);

    public async Task DownloadAsync(string path, string fileName, string contentType)
    {
        using var response = await SendForResponseAsync(HttpMethod.Get, path);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        await jsRuntime.InvokeVoidAsync("sessionPlanner.downloadFromBytes", fileName, contentType, Convert.ToBase64String(bytes));
    }

    public async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null, bool allowRetry = true)
    {
        using var response = await SendForResponseAsync(method, path, body, allowRetry);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default!;
        }

        var responseText = await response.Content.ReadAsStringAsync();

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

    private async Task<HttpResponseMessage> SendForResponseAsync(
        HttpMethod method,
        string path,
        object? body = null,
        bool allowRetry = true)
    {
        var token = await auth.GetFreshAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ApiException("Session expiree. Veuillez vous reconnecter.", 401);
        }

        var response = await SendOnceAsync(method, path, body, token);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && allowRetry)
        {
            response.Dispose();
            if (await auth.RefreshAsync())
            {
                token = await auth.GetFreshAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    response = await SendOnceAsync(method, path, body, token);
                }
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            var message = GetErrorMessage(responseText, response.ReasonPhrase);
            response.Dispose();
            throw new ApiException(message, (int)response.StatusCode, responseText);
        }

        return response;
    }

    private async Task<HttpResponseMessage> SendOnceAsync(HttpMethod method, string path, object? body, string token)
    {
        using var request = new HttpRequestMessage(method, path.TrimStart('/'));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonDefaults.Options),
                Encoding.UTF8,
                "application/json");
        }

        return await httpClient.SendAsync(request);
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
