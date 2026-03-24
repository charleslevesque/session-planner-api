using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using System.Security.Claims;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Dtos.Auth;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Dtos.RefreshTokens;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.OpenApi.Examples.Auth;
using SessionPlanner.Api.OpenApi.Examples.Common;
using SessionPlanner.Api.Common;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Creates a new account.
    /// </summary>
    /// <remarks>
    /// Registers a new user account and immediately returns JWT authentication tokens.
    /// </remarks>
    /// <param name="request">The registration details, including email, password, first name, and last name.</param>
    /// <returns>The created account's authentication tokens.</returns>
    /// <response code="201">The account was created successfully.</response>
    /// <response code="400">The request is invalid or the account could not be created.</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Register a new account",
        Description = "Creates a new account and returns an access token and refresh token."
    )]
    [SwaggerRequestExample(typeof(RegisterRequest), typeof(RegisterRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(AuthResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(AccountAlreadyExistsExample))]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(
                request.Email, request.Password, request.FirstName, request.LastName, request.Role);

            return Created(string.Empty, new AuthResponse(result.AccessToken, result.RefreshToken, result.ExpiresAtUtc));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message, ErrorCodes.Conflict));
        }
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <remarks>
    /// Validates the supplied email and password and returns a new access token and refresh token.
    /// </remarks>
    /// <param name="request">The user's login credentials.</param>
    /// <returns>A valid access token, refresh token, and expiration timestamp.</returns>
    /// <response code="200">Authentication succeeded.</response>
    /// <response code="401">The email or password is invalid.</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Authenticate a user",
        Description = "Authenticates a user with email and password and returns JWT tokens."
    )]
    [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AuthResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(InvalidCredentialsExample))]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result is null)
            return Unauthorized(new ApiErrorResponse("Invalid email or password.", ErrorCodes.InvalidCredentials));

        return Ok(new AuthResponse(result.AccessToken, result.RefreshToken, result.ExpiresAtUtc));
    }

    /// <summary>
    /// Returns the authenticated user's profile.
    /// </summary>
    /// <remarks>
    /// Retrieves the profile associated with the currently authenticated user.
    /// </remarks>
    /// <returns>The current user's basic profile information.</returns>
    /// <response code="200">The profile was retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated or the user could not be resolved.</response>
    [Authorize]
    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Get current user profile",
        Description = "Returns the profile information for the currently authenticated user."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(MeResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(CurrentUserNotFoundExample))]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new ApiErrorResponse(
                Error: "The current user could not be found.",
                Code: ErrorCodes.InvalidCredentials
            ));

        var user = await _authService.GetCurrentUserAsync(userId);

        if (user is null)
            return Unauthorized(new ApiErrorResponse(
                Error: "The current user could not be found.",
                Code: ErrorCodes.InvalidCredentials
        ));

        var role = user.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? string.Empty;
        var name = user.Personnel is not null
            ? $"{user.Personnel.FirstName} {user.Personnel.LastName}"
            : user.Username;

        return Ok(new MeResponse(user.Id, user.Username, name, role));
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <remarks>
    /// Issues a new access token and refresh token when the supplied refresh token is valid.
    /// </remarks>
    /// <param name="request">The refresh token payload.</param>
    /// <returns>A new access token, refresh token, and expiration timestamp.</returns>
    /// <response code="200">The token was refreshed successfully.</response>
    /// <response code="401">The refresh token is invalid or expired.</response>
    [HttpPost("refresh")]
    [SwaggerOperation(
        Summary = "Refresh authentication tokens",
        Description = "Uses a valid refresh token to issue a new access token and refresh token."
    )]
    [SwaggerRequestExample(typeof(RefreshTokenRequest), typeof(RefreshTokenRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AuthResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(InvalidRefreshTokenExample))]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result is null)

            return Unauthorized(new ApiErrorResponse(
                Error: "The user could not be found.",
                Code: ErrorCodes.Unauthorized)
        );

        return Ok(new AuthResponse(result.AccessToken, result.RefreshToken, result.ExpiresAtUtc));
    }

    /// <summary>
    /// Revokes a refresh token.
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
    /// <remarks>
    /// Invalidates the supplied refresh token to log the current user out.
    /// </remarks>
    /// <param name="request">The refresh token to revoke.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The refresh token was revoked successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [Authorize]
    [HttpPost("logout")]
    [SwaggerOperation(
        Summary = "Log out a user",
        Description = "Revokes the supplied refresh token and ends the authenticated session."
    )]
    [SwaggerRequestExample(typeof(RefreshTokenRequest), typeof(RefreshTokenRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}
