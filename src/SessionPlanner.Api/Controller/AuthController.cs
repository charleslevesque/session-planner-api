using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Auth;
using SessionPlanner.Infrastructure.Auth;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.Login;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using SessionPlanner.Api.Auth;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[AllowAnonymous]
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

        var loginResponse = _jwtTokenService.CreateToken(user, roles, permissions);

        return Ok(loginResponse);
    }
}