using SessionPlanner.Web.Models;

namespace SessionPlanner.Web.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> RegisterAsync(RegisterRequest req);
    Task<string?> GetTokenAsync();
    Task<MeResponse?> GetStoredUserAsync();
    Task RestoreSessionAsync();
    Task<bool> RefreshTokenAsync();
    event Action OnAuthStateChanged;
}
