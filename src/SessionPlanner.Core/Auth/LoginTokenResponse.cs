namespace SessionPlanner.Core.Auth;

public record LoginTokenResponse(string AccessToken, DateTime ExpiresAtUtc);