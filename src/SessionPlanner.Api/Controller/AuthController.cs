using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Auth;
using SessionPlanner.Infrastructure.Auth;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.Login;
using SessionPlanner.Api.Dtos.RefreshTokens;
using SessionPlanner.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using SessionPlanner.Api.Auth;
using System.Security.Cryptography;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJWTTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;

    public AuthController(AppDbContext db, IJWTTokenService jwtTokenService, IPasswordService passwordService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
        .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null)
            return Unauthorized();

        var validPassword = _passwordService.VerifyPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (!validPassword)
            return Unauthorized();

        var roles = user.UserRoles
            .Select(x => x.Role.Name)
            .Distinct()
            .ToList();

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToList();

        var (accessToken, expiresAtUtc) = _jwtTokenService.CreateToken(user, roles, permissions);

        var refreshTokenValue = GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        });

        await _db.SaveChangesAsync();

        return Ok(new LoginTokenResponse(
            accessToken,
            refreshTokenValue,
            expiresAtUtc
        ));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var refreshToken = await _db.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshToken is null || !refreshToken.IsActive || !refreshToken.User.IsActive)
            return Unauthorized();

        var user = refreshToken.User;

        var roles = user.UserRoles
            .Select(x => x.Role.Name)
            .Distinct()
            .ToList();

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToList();

        var (accessToken, expiresAtUtc) =
            _jwtTokenService.CreateToken(user, roles, permissions);

        refreshToken.RevokedAtUtc = DateTime.UtcNow;

        var newRefreshTokenValue = GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenValue,
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync();

        return Ok(new LoginTokenResponse(accessToken, newRefreshTokenValue, expiresAtUtc));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshToken is null)
            return NoContent();

        if (!refreshToken.IsRevoked)
        {
            refreshToken.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}