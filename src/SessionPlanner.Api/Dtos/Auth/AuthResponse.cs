namespace SessionPlanner.Api.Dtos.Auth;

public record AuthResponse(string Token, string RefreshToken, DateTime ExpiresAt);
