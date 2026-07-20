using Microsoft.JSInterop;

namespace SessionPlanner.Web.Services;

public sealed class SessionStorageService(IJSRuntime jsRuntime)
{
    public ValueTask<string?> GetAsync(string key) =>
        jsRuntime.InvokeAsync<string?>("sessionPlanner.storage.get", key);

    public ValueTask SetAsync(string key, string value) =>
        jsRuntime.InvokeVoidAsync("sessionPlanner.storage.set", key, value);

    public ValueTask RemoveAsync(string key) =>
        jsRuntime.InvokeVoidAsync("sessionPlanner.storage.remove", key);
}
