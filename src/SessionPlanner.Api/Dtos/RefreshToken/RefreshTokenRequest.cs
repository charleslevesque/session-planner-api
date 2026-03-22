namespace SessionPlanner.Api.Dtos.RefreshTokens;
/// <summary>The refresh token to revoke or exchange.</summary>
/// <example>sample-refresh-token-123</example>
public record RefreshTokenRequest(string RefreshToken);