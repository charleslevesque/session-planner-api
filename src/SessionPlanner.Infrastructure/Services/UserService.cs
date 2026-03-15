using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IPasswordService _passwordService;

    public UserService(AppDbContext db, IPasswordService passwordService)
    {
        _db = db;
        _passwordService = passwordService;
    }

    private IQueryable<User> ActiveUsersWithRoles()
    {
        return _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive);
    }

    public async Task<List<User>> GetAllActiveWithRolesAsync()
    {
        return await ActiveUsersWithRoles().ToListAsync();
    }

    public async Task<User?> GetByIdActiveWithRolesAsync(int id)
    {
        return await ActiveUsersWithRoles()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByIdWithRolesAsync(int id)
    {
        return await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<CreateUserResult> CreateAsync(string username, string password, string? roleName)
    {
        var existingUser = await _db.Users
            .AnyAsync(u => u.Username == username);

        if (existingUser)
            return new CreateUserResult(CreateUserStatus.UsernameAlreadyExists, null);

        Role role;

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            role = await _db.Roles
                .FirstAsync(r => r.Name == roleName);
        }
        else
        {
            role = await _db.Roles
                .FirstAsync(r => r.Name == Roles.Teacher);
        }

        var user = new User
        {
            Username = username,
            IsActive = true,
        };

        user.PasswordHash = _passwordService.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });

        await _db.SaveChangesAsync();

        var createdUser = await ActiveUsersWithRoles()
            .FirstAsync(u => u.Id == user.Id);

        return new CreateUserResult(CreateUserStatus.Success, createdUser);
    }

    public async Task<UpdateUserRoleStatus> UpdateRoleAsync(int id, string roleName)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return UpdateUserRoleStatus.UserNotFound;

        var role = await _db.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName);

        if (role is null)
            return UpdateUserRoleStatus.RoleNotFound;

        _db.UserRoles.RemoveRange(user.UserRoles);

        _db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });

        await _db.SaveChangesAsync();

        return UpdateUserRoleStatus.Success;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user is null)
            return false;

        user.IsActive = false;
        await _db.SaveChangesAsync();

        return true;
    }
}