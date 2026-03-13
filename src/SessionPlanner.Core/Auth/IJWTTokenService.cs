using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Auth;

public interface IJWTTokenService
{
    LoginTokenResponse CreateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
}