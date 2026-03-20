using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Users;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SessionPlanner.Core.Interfaces;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [HasPermission(Permissions.Users.Read)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var users = await _userService.GetAllActiveWithRolesAsync();
        return Ok(users.Select(u => u.ToResponse()));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var user = await _userService.GetByIdWithRolesAsync(userId);

        if (user is null)
            return NotFound();

        return Ok(user.ToResponse());
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Users.Read)]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await _userService.GetByIdActiveWithRolesAsync(id);

        if (user is null)
            return NotFound();

        return Ok(user.ToResponse());
    }

    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var result = await _userService.CreateAsync(request.Username, request.Password, request.RoleName);

        if (result.Status == CreateUserStatus.UsernameAlreadyExists)
            return BadRequest("Username already exists.");

        var createdUser = result.User!;

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdUser.Id },
            createdUser.ToResponse()
        );
    }

    [HttpPut("{id:int}/role")]
    [HasPermission(Permissions.Users.Update)]
    public async Task<IActionResult> UpdateRole(int id, UpdateUserRequest request)
    {
        var status = await _userService.UpdateRoleAsync(id, request.RoleName);

        if (status == UpdateUserRoleStatus.UserNotFound)
            return NotFound();

        if (status == UpdateUserRoleStatus.RoleNotFound)
            return BadRequest($"Role '{request.RoleName}' does not exist.");

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
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

}