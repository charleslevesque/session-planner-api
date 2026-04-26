using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext db,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IConfiguration configuration)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user is null)
            return new LoginResult(LoginStatus.InvalidCredentials);

        if (!await _userManager.CheckPasswordAsync(user, password))
            return new LoginResult(LoginStatus.InvalidCredentials);

        if (!user.IsActive)
            return new LoginResult(LoginStatus.AccountDeactivated);

        return new LoginResult(LoginStatus.Success, await GenerateTokensAsync(user));
    }

    public async Task<AppUser?> GetCurrentUserAsync(int userId)
    {
        return await _userManager.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Personnel)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
    }

    public async Task<LoginTokenResponse?> RefreshTokenAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(rt => rt.User)
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
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null || !user.IsActive)
            return ChangePasswordStatus.UserNotFound;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            return ChangePasswordStatus.InvalidCurrentPassword;

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

    private async Task<LoginTokenResponse> GenerateTokensAsync(AppUser user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is missing.");

        var expiryMinutesValue = _configuration["Jwt:ExpiryMinutes"]
            ?? throw new InvalidOperationException("Jwt:ExpiryMinutes is missing.");

        if (!int.TryParse(expiryMinutesValue, out var expiryMinutes))
            throw new InvalidOperationException("Jwt:ExpiryMinutes must be a valid integer.");

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var roleNames = await _userManager.GetRolesAsync(user);

        var permissions = new List<string>();
        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                permissions.AddRange(claims
                    .Where(c => c.Type == "perm")
                    .Select(c => c.Value));
            }
        }

        var jwtClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        foreach (var role in roleNames.Distinct())
            jwtClaims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions.Distinct())
            jwtClaims.Add(new Claim("perm", permission));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: jwtClaims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _db.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        });

        await _db.SaveChangesAsync();

        return new LoginTokenResponse(accessToken, refreshTokenValue, expiresAt);
    }
}
