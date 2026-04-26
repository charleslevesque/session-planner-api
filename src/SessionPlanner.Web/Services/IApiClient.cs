namespace SessionPlanner.Web.Services;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string path);
    Task<TRes?> PostAsync<TReq, TRes>(string path, TReq body);
    Task PostVoidAsync<TReq>(string path, TReq body);
    Task PostVoidAsync(string path);
    Task<TRes?> PutAsync<TReq, TRes>(string path, TReq body);
    Task DeleteAsync(string path);
}
