using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public enum ChangePasswordStatus
{
    Success,
    UserNotFound,
    InvalidCurrentPassword
}

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    AccountDeactivated
}

public record LoginResult(LoginStatus Status, LoginTokenResponse? Tokens = null);

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string username, string password);
    Task<AppUser?> GetCurrentUserAsync(int userId);
    Task<LoginTokenResponse?> RefreshTokenAsync(string refreshToken);
    Task<ChangePasswordStatus> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task LogoutAsync(string refreshToken);
}
