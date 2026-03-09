using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Auth;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string hashedPassword, string providedPassword);
}