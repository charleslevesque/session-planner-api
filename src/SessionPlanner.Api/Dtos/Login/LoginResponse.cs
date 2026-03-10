namespace SessionPlanner.Api.Dtos.Login;

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);