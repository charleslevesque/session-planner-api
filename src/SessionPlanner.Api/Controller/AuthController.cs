using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using System.Security.Claims;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Dtos.Auth;
using SessionPlanner.Api.Dtos.RefreshTokens;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Creates a new teacher account.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(
                request.Email, request.Password, request.FirstName, request.LastName);

            return Created(string.Empty, new AuthResponse(result.AccessToken, result.RefreshToken, result.ExpiresAtUtc));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(Dtos.Auth.LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result is null)
            return Unauthorized(new { error = "Invalid email or password." });

        return Ok(new AuthResponse(result.AccessToken, result.RefreshToken, result.ExpiresAtUtc));
    }

    /// <summary>
    /// Returns the authenticated user's profile.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(userId);

        if (user is null)
            return Unauthorized();

        var role = user.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? string.Empty;
        var name = user.Personnel is not null
            ? $"{user.Personnel.FirstName} {user.Personnel.LastName}"
            : user.Username;

        return Ok(new MeResponse(user.Id, user.Username, name, role));
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result is null)
            return Unauthorized();

        return Ok(new AuthResponse(result.AccessToken, result.RefreshToken, result.ExpiresAtUtc));
    }

    /// <summary>
    /// Changes the authenticated user's password.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var status = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

        if (status == ChangePasswordStatus.UserNotFound)
            return Unauthorized();

        if (status == ChangePasswordStatus.InvalidCurrentPassword)
            return BadRequest(new { error = "Current password is incorrect." });

        return NoContent();
    }

    /// <summary>
    /// Revokes a refresh token (logout).
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}
