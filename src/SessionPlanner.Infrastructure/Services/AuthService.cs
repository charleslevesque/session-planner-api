using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJWTTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;

    public AuthService(AppDbContext db, IJWTTokenService jwtTokenService, IPasswordService passwordService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
    }

    public async Task<LoginTokenResponse> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        var exists = await _db.Users.AnyAsync(u => u.Username == email);
        if (exists)
            throw new InvalidOperationException("A user with this email already exists.");

        var teacherRole = await _db.Roles.FirstAsync(r => r.Name == Roles.Professor);

        var user = new User
        {
            Username = email,
            IsActive = true,
        };
        user.PasswordHash = _passwordService.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = teacherRole.Id });

        var personnel = await _db.Personnel
            .FirstOrDefaultAsync(p => p.Email == email);

        if (personnel is null)
        {
            personnel = new Personnel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Function = PersonnelFunction.Professor,
            };
            _db.Personnel.Add(personnel);
            await _db.SaveChangesAsync();
        }

        user.PersonnelId = personnel.Id;
        await _db.SaveChangesAsync();

        return await GenerateTokensAsync(user);
    }

    public async Task<LoginTokenResponse?> LoginAsync(string username, string password)
    {
        var user = await UsersWithRolesAndPermissions()
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
            return null;

        if (!_passwordService.VerifyPassword(user, user.PasswordHash, password))
            return null;

        return await GenerateTokensAsync(user);
    }

    public async Task<User?> GetCurrentUserAsync(int userId)
    {
        return await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Personnel)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
    }

    public async Task<LoginTokenResponse?> RefreshTokenAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token is null || !token.IsActive || !token.User.IsActive)
            return null;

        token.RevokedAtUtc = DateTime.UtcNow;

        var result = await GenerateTokensAsync(token.User);
        await _db.SaveChangesAsync();

        return result;
    }

    public async Task<ChangePasswordStatus> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user is null)
            return ChangePasswordStatus.UserNotFound;

        if (!_passwordService.VerifyPassword(user, user.PasswordHash, currentPassword))
            return ChangePasswordStatus.InvalidCurrentPassword;

        user.PasswordHash = _passwordService.HashPassword(user, newPassword);
        await _db.SaveChangesAsync();

        return ChangePasswordStatus.Success;
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token is not null && !token.IsRevoked)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    private IQueryable<User> UsersWithRolesAndPermissions()
    {
        return _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions);
    }

    private async Task<LoginTokenResponse> GenerateTokensAsync(User user)
    {
        if (!user.UserRoles.Any())
        {
            user = await UsersWithRolesAndPermissions().FirstAsync(u => u.Id == user.Id);
        }

        var roles = user.UserRoles.Select(x => x.Role.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToList();

        var (accessToken, expiresAtUtc) = _jwtTokenService.CreateToken(user, roles, permissions);

        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _db.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        });

        await _db.SaveChangesAsync();

        return new LoginTokenResponse(accessToken, refreshTokenValue, expiresAtUtc);
    }
}
