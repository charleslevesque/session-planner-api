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

    public async Task<List<User>> GetAllWithRolesAsync(bool includeInactive = false)
    {
        var query = _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);

        return includeInactive
            ? await query.ToListAsync()
            : await query.Where(u => u.IsActive).ToListAsync();
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
                .FirstAsync(r => r.Name == Roles.Professor);
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
            Roles.Professor => PersonnelFunction.Professor,
            Roles.LabInstructor => PersonnelFunction.LabInstructor,
            Roles.CourseInstructor => PersonnelFunction.CourseInstructor,
            _ => PersonnelFunction.Professor,
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

    public async Task<UpdateCurrentUserEmailStatus> UpdateCurrentUserEmailAsync(int userId, string newEmail, string currentPassword)
    {
        var normalizedEmail = (newEmail ?? string.Empty).Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user is null)
            return UpdateCurrentUserEmailStatus.UserNotFound;

        var isAdmin = user.UserRoles.Any(ur => ur.Role.Name == Roles.Admin);
        if (!isAdmin)
            return UpdateCurrentUserEmailStatus.ForbiddenForNonAdmin;

        if (!_passwordService.VerifyPassword(user, user.PasswordHash, currentPassword))
            return UpdateCurrentUserEmailStatus.InvalidCurrentPassword;

        var emailAlreadyExists = await _db.Users
            .AnyAsync(u => u.Id != userId && u.Username.ToLower() == normalizedEmail);

        if (emailAlreadyExists)
            return UpdateCurrentUserEmailStatus.EmailAlreadyExists;

        user.Username = normalizedEmail;

        if (user.PersonnelId.HasValue)
        {
            var personnel = await _db.Personnel.FirstOrDefaultAsync(p => p.Id == user.PersonnelId.Value);
            if (personnel is not null)
            {
                personnel.Email = normalizedEmail;
            }
        }

        await _db.SaveChangesAsync();
        return UpdateCurrentUserEmailStatus.Success;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return false;

        var isAdmin = user.UserRoles.Any(ur => ur.Role.Name == Roles.Admin);
        if (isAdmin)
            return false;

        var personnelId = user.PersonnelId;

        _db.UserRoles.RemoveRange(user.UserRoles);

        var userPermissions = await _db.UserPermissions
            .Where(up => up.UserId == id)
            .ToListAsync();
        _db.UserPermissions.RemoveRange(userPermissions);

        var reviewedNeeds = await _db.TeachingNeeds
            .Where(tn => tn.ReviewedByUserId == id)
            .ToListAsync();
        foreach (var need in reviewedNeeds)
        {
            need.ReviewedByUserId = null;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        if (personnelId.HasValue)
        {
            var teachingNeeds = await _db.TeachingNeeds
                .Include(tn => tn.Items)
                .Where(tn => tn.PersonnelId == personnelId.Value)
                .ToListAsync();

            foreach (var need in teachingNeeds)
            {
                _db.TeachingNeedItems.RemoveRange(need.Items);
            }
            _db.TeachingNeeds.RemoveRange(teachingNeeds);

            var coursePersonnels = await _db.Set<CoursePersonnel>()
                .Where(cp => cp.PersonnelId == personnelId.Value)
                .ToListAsync();
            _db.Set<CoursePersonnel>().RemoveRange(coursePersonnels);

            var personnel = await _db.Personnel.FindAsync(personnelId.Value);
            if (personnel is not null)
            {
                _db.Personnel.Remove(personnel);
            }

            await _db.SaveChangesAsync();
        }

        return true;
    }

    public async Task<DeactivateUserStatus> DeactivateAsync(int id)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return DeactivateUserStatus.UserNotFound;

        if (user.UserRoles.Any(ur => ur.Role.Name == Roles.Admin))
            return DeactivateUserStatus.CannotDeactivateAdmin;

        user.IsActive = false;

        var refreshTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == id && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();
        foreach (var token in refreshTokens)
            token.RevokedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return DeactivateUserStatus.Success;
    }

    public async Task<ReactivateUserStatus> ReactivateAsync(int id)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return ReactivateUserStatus.UserNotFound;

        if (user.IsActive)
            return ReactivateUserStatus.AlreadyActive;

        user.IsActive = true;
        await _db.SaveChangesAsync();
        return ReactivateUserStatus.Success;
    }

    public async Task<User?> GetByIdWithFullProfileAsync(int id)
    {
        return await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Personnel)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}