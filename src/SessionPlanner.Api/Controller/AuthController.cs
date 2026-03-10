using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Auth;
using SessionPlanner.Infrastructure.Auth;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.Login;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

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
            .Include(x => x.UserPermissions)
                .ThenInclude(x => x.Permission)
            .SingleOrDefaultAsync(x => x.Username == request.Username);

        if (user is null)
            return Unauthorized();

        var validPassword = _passwordService.VerifyPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (!validPassword)
            return Unauthorized();

        var permissions = user.UserPermissions.Select(x => x.Permission.Name);

        var loginResponse = _jwtTokenService.CreateToken(user, permissions);

        return Ok(loginResponse);
    }
}