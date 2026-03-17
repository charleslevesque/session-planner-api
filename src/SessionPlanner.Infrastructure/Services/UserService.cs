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

        var existingPersonnel = await _db.Personnel
            .FirstOrDefaultAsync(p => p.Email == username);

        if (existingPersonnel is not null)
        {
            user.PersonnelId = existingPersonnel.Id;
        }
        else
        {
            var localPart = username.Split('@')[0];
            var nameParts = localPart
                .Split(new[] { '.', '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var firstName = nameParts.Length > 0 ? ToTitle(nameParts[0]) : "User";
            var lastName = nameParts.Length > 1 ? ToTitle(nameParts[1]) : "Account";

            var personnel = new Personnel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = username,
                Function = MapRoleToPersonnelFunction(role.Name),
            };

            _db.Personnel.Add(personnel);
            await _db.SaveChangesAsync();
            user.PersonnelId = personnel.Id;
        }

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

    private static PersonnelFunction MapRoleToPersonnelFunction(string roleName)
    {
        return roleName switch
        {
            Roles.Teacher => PersonnelFunction.Professor,
            Roles.Technician => PersonnelFunction.LabInstructor,
            _ => PersonnelFunction.CourseInstructor,
        };
    }

    private static string ToTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "User";
        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
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

    public async Task<UpdateUserPasswordStatus> UpdatePasswordAsync(int id, string newPassword)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

        if (user is null)
            return UpdateUserPasswordStatus.UserNotFound;

        user.PasswordHash = _passwordService.HashPassword(user, newPassword);
        await _db.SaveChangesAsync();

        return UpdateUserPasswordStatus.Success;
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