namespace SessionPlanner.Core.Auth;

public record LoginTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);