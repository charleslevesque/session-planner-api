using Microsoft.AspNetCore.Identity;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Auth;

namespace SessionPlanner.Infrastructure.Auth;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password)
        => _hasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}