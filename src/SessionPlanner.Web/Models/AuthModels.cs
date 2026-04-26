namespace SessionPlanner.Web.Models;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string? Role);
public record AuthResponse(string Token, string RefreshToken, string ExpiresAt);
public record RefreshTokenRequest(string RefreshToken);
public record MeResponse(int Id, string Email, string Name, string Role);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record UpdateEmailRequest(string NewEmail, string CurrentPassword);
