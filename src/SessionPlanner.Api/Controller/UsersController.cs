using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.Users;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordService _passwordService;

    public UsersController(AppDbContext db, IPasswordService passwordService)
    {
        _db = db;
        _passwordService = passwordService;
    }

    private IQueryable<User> UsersWithRoles()
    {
        return _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role).
                    Where(s => s.IsActive);
    }

    [HttpGet]
    [HasPermission(Permissions.Users.Read)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var users = await UsersWithRoles().ToListAsync();
        return Ok(users.Select(u => u.ToResponse()));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return NotFound();

        return Ok(user.ToResponse());
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Users.Read)]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await UsersWithRoles()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound();

        return Ok(user.ToResponse());
    }

    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var existingUser = await _db.Users
            .AnyAsync(u => u.Username == request.Username);

        if (existingUser)
            return BadRequest("Username already exists.");


        Role role;

        if (!string.IsNullOrWhiteSpace(request.RoleName))
        {
            role = await _db.Roles
                .FirstAsync(r => r.Name == request.RoleName);
        }
        else
        {
            role = await _db.Roles
                .FirstAsync(r => r.Name == Roles.Teacher);
        }

        var user = new User
        {
            Username = request.Username,
            IsActive = true,
        };

        user.PasswordHash = _passwordService.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });

        await _db.SaveChangesAsync();

        var createdUser = await UsersWithRoles()
            .FirstAsync(u => u.Id == user.Id);

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
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound();

        var role = await _db.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName);

        if (role is null)
            return BadRequest($"Role '{request.RoleName}' does not exist.");

        _db.UserRoles.RemoveRange(user.UserRoles);

        _db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Users.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user is null)
            return NotFound();

        user.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }

}