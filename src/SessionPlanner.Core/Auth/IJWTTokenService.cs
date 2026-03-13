using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Auth;

public interface IJWTTokenService
{
    (string AccessToken, DateTime ExpiresAtUtc) CreateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);

}