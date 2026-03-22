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

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Users.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeactivateAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

}