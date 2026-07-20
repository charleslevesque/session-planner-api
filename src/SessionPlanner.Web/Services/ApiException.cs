namespace SessionPlanner.Web.Services;

public sealed class ApiException(string message, int statusCode, string? responseBody = null)
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string? ResponseBody { get; } = responseBody;
}
