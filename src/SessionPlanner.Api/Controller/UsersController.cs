using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Users;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.Users;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;
namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retrieves all active users with their roles.
    /// </summary>
    /// <returns>A list of active users.</returns>
    /// <response code="200">The users were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read users.</response>
    [HttpGet]
    [HasPermission(Permissions.Users.Read)]
    [SwaggerOperation(
        Summary = "Get all users",
        Description = "Returns all active users with their assigned roles."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var users = includeInactive
            ? await _userService.GetAllWithRolesAsync(includeInactive: true)
            : await _userService.GetAllActiveWithRolesAsync();
        return Ok(users.Select(u => u.ToResponse()));
    }

    /// <summary>
    /// Retrieves an active user by identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The matching active user.</returns>
    /// <response code="200">The user was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read users.</response>
    /// <response code="404">No active user exists with the supplied identifier.</response>
    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Users.Read)]
    [SwaggerOperation(
        Summary = "Get a user by id",
        Description = "Returns a single active user by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await _userService.GetByIdWithRolesAsync(id);

        if (user is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "User not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No user exists with id {id}."
            ));
        }

        return Ok(user.ToResponse());
    }

    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="request">The user details, including their username, password and role.</param>
    /// <returns>The newly created user.</returns>
    /// <response code="201">The user was created successfully.</response>
    /// <response code="400">The request is invalid or the username already exists.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create users.</response>
    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
    [SwaggerOperation(
        Summary = "Create a user",
        Description = "Creates a new user and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateUserRequest), typeof(CreateUserRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(UserResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(UsernameAlreadyExistsExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var result = await _userService.CreateAsync(request.Username, request.Password, request.RoleName);

        if (result.Status == CreateUserStatus.UsernameAlreadyExists)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Username already exists.",
                Code: ErrorCodes.Conflict,
                Details: $"The username '{request.Username}' is already in use."
            ));
        }

        var createdUser = result.User!;

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdUser.Id },
            createdUser.ToResponse()
        );
    }

    /// <summary>
    /// Updates a user's role.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The new role information.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The user's role was updated successfully.</response>
    /// <response code="400">The supplied role does not exist.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update users.</response>
    /// <response code="404">No user exists with the supplied identifier.</response>
    [HttpPut("{id:int}/role")]
    [HasPermission(Permissions.Users.Update)]
    [SwaggerOperation(
        Summary = "Update a user's role",
        Description = "Updates the assigned role for an existing user."
    )]
    [SwaggerRequestExample(typeof(UpdateUserRequest), typeof(UpdateUserRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(int id, UpdateUserRequest request)
    {
        var status = await _userService.UpdateRoleAsync(id, request.RoleName);

        if (status == UpdateUserRoleStatus.UserNotFound)
        {
            return NotFound(new ApiErrorResponse(
                Error: "User not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No user exists with id {id}."
            ));
        }

        if (status == UpdateUserRoleStatus.RoleNotFound)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Role not found.",
                Code: ErrorCodes.RoleNotFound,
                Details: $"Role '{request.RoleName}' does not exist."
            ));
        }

        return NoContent();
    }

    [HttpPut("{id:int}/password")]
    [HasPermission(Permissions.Users.Update)]
    public async Task<IActionResult> UpdatePassword(int id, UpdateUserPasswordRequest request)
    {
        var status = await _userService.UpdatePasswordAsync(id, request.NewPassword);

        if (status == UpdateUserPasswordStatus.UserNotFound)
            return NotFound();

        return NoContent();
    }

    [HttpPut("me/email")]
    public async Task<IActionResult> UpdateCurrentUserEmail(UpdateCurrentUserEmailRequest request)
    {
        if (!User.IsInRole(Roles.Admin))
            return Forbid();

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var status = await _userService.UpdateCurrentUserEmailAsync(userId, request.NewEmail, request.CurrentPassword);

        if (status == UpdateCurrentUserEmailStatus.UserNotFound)
            return Unauthorized();

        if (status == UpdateCurrentUserEmailStatus.ForbiddenForNonAdmin)
            return Forbid();

        if (status == UpdateCurrentUserEmailStatus.InvalidCurrentPassword)
            return BadRequest(new { error = "Current password is incorrect." });

        if (status == UpdateCurrentUserEmailStatus.EmailAlreadyExists)
            return BadRequest(new { error = "Email is already in use." });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Users.Delete)]
    [SwaggerOperation(
        Summary = "Delete a user",
        Description = "Deactivates an existing user."
    )]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "User not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No user exists with id {id}."
            ));
        }

        return NoContent();
    }

    // POST /api/v1/users/{id}/deactivate
    [HttpPost("{id:int}/deactivate")]
    [HasPermission(Permissions.Users.Update)]
    [SwaggerOperation(
        Summary = "Deactivate a user account",
        Description = "Sets the user as inactive. The account data and history are preserved. Revokes active sessions."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id)
    {
        var status = await _userService.DeactivateAsync(id);

        if (status == DeactivateUserStatus.UserNotFound)
            return NotFound(new ApiErrorResponse("User not found.", ErrorCodes.NotFound, $"No user exists with id {id}."));

        if (status == DeactivateUserStatus.CannotDeactivateAdmin)
            return BadRequest(new ApiErrorResponse("Cannot deactivate an admin account.", ErrorCodes.Conflict));

        return NoContent();
    }

    // POST /api/v1/users/{id}/reactivate
    [HttpPost("{id:int}/reactivate")]
    [HasPermission(Permissions.Users.Update)]
    [SwaggerOperation(
        Summary = "Reactivate a user account",
        Description = "Restores a previously deactivated user account. The user will be able to log in again."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(int id)
    {
        var status = await _userService.ReactivateAsync(id);

        if (status == ReactivateUserStatus.UserNotFound)
            return NotFound(new ApiErrorResponse("User not found.", ErrorCodes.NotFound, $"No user exists with id {id}."));

        if (status == ReactivateUserStatus.AlreadyActive)
            return BadRequest(new ApiErrorResponse("User is already active.", ErrorCodes.Conflict));

        return NoContent();
    }

}