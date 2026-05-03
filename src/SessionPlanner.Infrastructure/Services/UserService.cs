using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Enums;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class UserService(AppDbContext db, UserManager<AppUser> userManager) : IUserService
{
    private readonly AppDbContext _db = db;
    private readonly UserManager<AppUser> _userManager = userManager;

    private IQueryable<AppUser> ActiveUsersWithRoles()
    => _userManager.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .Where(u => u.IsActive);

    public async Task<List<AppUser>> GetAllActiveWithRolesAsync()
    => await ActiveUsersWithRoles().ToListAsync();

    public async Task<List<AppUser>> GetAllWithRolesAsync(bool includeInactive = false)
    {
        var query = _userManager.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);

        return includeInactive
            ? await query.ToListAsync()
            : await query.Where(u => u.IsActive).ToListAsync();
    }

    public async Task<AppUser?> GetByIdActiveWithRolesAsync(int id)
    => await ActiveUsersWithRoles()
        .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<AppUser?> GetByIdWithRolesAsync(int id)
    => await _userManager.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<CreateUserResult> CreateAsync(string username, string password, string? roleName)
    {
        var existingUser = await _userManager.FindByNameAsync(username);

        if (existingUser is not null)
            return new CreateUserResult(CreateUserStatus.UsernameAlreadyExists, null);

        var resolvedRole = ResolveRoleName(roleName);

        var user = new AppUser
        {
            UserName = username,
            IsActive = true,
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return new CreateUserResult(CreateUserStatus.UsernameAlreadyExists, null);

        await _userManager.AddToRoleAsync(user, resolvedRole);

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
                Function = MapRoleToPersonnelFunction(resolvedRole),
            };

            _db.Personnel.Add(personnel);
            await _db.SaveChangesAsync();
            user.PersonnelId = personnel.Id;
        }

        await _userManager.UpdateAsync(user);

        var createdUser = await _userManager.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == user.Id);

        return new CreateUserResult(CreateUserStatus.Success, createdUser);
    }

    private static string ResolveRoleName(string? roleName)
    => roleName?.Trim()?.ToLowerInvariant() switch
    {
        Roles.LabInstructor or "labinstructor" => Roles.LabInstructor,
        Roles.CourseInstructor or "courseinstructor" => Roles.CourseInstructor,
        Roles.Professor => Roles.Professor,
        _ => Roles.Professor,
    };

    private static PersonnelFunction MapRoleToPersonnelFunction(string roleName)
    => roleName switch
    {
        Roles.Professor => PersonnelFunction.Professor,
        Roles.LabInstructor => PersonnelFunction.LabInstructor,
        Roles.CourseInstructor => PersonnelFunction.CourseInstructor,
        _ => PersonnelFunction.Professor,
    };

    private static string ToTitle(string value)
    => string.IsNullOrWhiteSpace(value)
        ? "User"
        : char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();

    public async Task<UpdateUserRoleStatus> UpdateRoleAsync(int id, string roleName)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
            return UpdateUserRoleStatus.UserNotFound;

        var roleExists = await _db.Roles.AnyAsync(r => r.Name == roleName);
        if (!roleExists)
            return UpdateUserRoleStatus.RoleNotFound;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, roleName);

        return UpdateUserRoleStatus.Success;
    }

    public async Task<UpdateUserPasswordStatus> UpdatePasswordAsync(int id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null || !user.IsActive)
            return UpdateUserPasswordStatus.UserNotFound;

        await _userManager.RemovePasswordAsync(user);
        await _userManager.AddPasswordAsync(user, newPassword);

        return UpdateUserPasswordStatus.Success;
    }

    public async Task<UpdateCurrentUserEmailStatus> UpdateCurrentUserEmailAsync(int userId, string newEmail, string currentPassword)
    {
        var normalizedEmail = (newEmail ?? string.Empty).Trim().ToLowerInvariant();

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null || !user.IsActive)
            return UpdateCurrentUserEmailStatus.UserNotFound;

        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        if (!isAdmin)
            return UpdateCurrentUserEmailStatus.ForbiddenForNonAdmin;

        if (!await _userManager.CheckPasswordAsync(user, currentPassword))
            return UpdateCurrentUserEmailStatus.InvalidCurrentPassword;

        var duplicate = await _userManager.FindByNameAsync(normalizedEmail);
        if (duplicate is not null && duplicate.Id != userId)
            return UpdateCurrentUserEmailStatus.EmailAlreadyExists;

        // Update the personnel email in the same tracked context so both changes
        // are written in the single SaveChangesAsync triggered by SetUserNameAsync.
        if (user.PersonnelId.HasValue)
        {
            var personnel = await _db.Personnel.FirstOrDefaultAsync(p => p.Id == user.PersonnelId.Value);
            personnel?.Email = normalizedEmail;
        }

        try
        {
            await _userManager.SetUserNameAsync(user, normalizedEmail);
        }
        catch
        {
            // TODO: Log the exception (not implemented here)
            return UpdateCurrentUserEmailStatus.FailedToUpdateEmail;
        }

        return UpdateCurrentUserEmailStatus.Success;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
            return false;

        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        if (isAdmin)
            return false;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var personnelId = user.PersonnelId;
        var reviewedNeeds = await _db.TeachingNeeds
            .Where(tn => tn.ReviewedByUserId == id)
            .ToListAsync();
        foreach (var need in reviewedNeeds)
            need.ReviewedByUserId = null;

        await _db.SaveChangesAsync();
        await _userManager.DeleteAsync(user);

        if (personnelId.HasValue)
        {
            var teachingNeeds = await _db.TeachingNeeds
                .Include(tn => tn.Items)
                .Where(tn => tn.PersonnelId == personnelId.Value)
                .ToListAsync();

            foreach (var need in teachingNeeds)
                _db.TeachingNeedItems.RemoveRange(need.Items);

            _db.TeachingNeeds.RemoveRange(teachingNeeds);

            var coursePersonnels = await _db.CoursePersonnels
                .Where(cp => cp.PersonnelId == personnelId.Value)
                .ToListAsync();
            _db.CoursePersonnels.RemoveRange(coursePersonnels);

            var personnel = await _db.Personnel.FindAsync(personnelId.Value);
            if (personnel is not null)
                _db.Personnel.Remove(personnel);

            await _db.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        return true;
    }

    public async Task<DeactivateUserStatus> DeactivateAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
            return DeactivateUserStatus.UserNotFound;

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            return DeactivateUserStatus.CannotDeactivateAdmin;

        user.IsActive = false;

        var refreshTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == id && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();
        foreach (var token in refreshTokens)
            token.RevokedAtUtc = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        await _db.SaveChangesAsync();

        return DeactivateUserStatus.Success;
    }

    public async Task<ReactivateUserStatus> ReactivateAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
            return ReactivateUserStatus.UserNotFound;

        if (user.IsActive)
            return ReactivateUserStatus.AlreadyActive;

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        return ReactivateUserStatus.Success;
    }

    public async Task<AppUser?> GetByIdWithFullProfileAsync(int id)
    => await _userManager.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .Include(u => u.Personnel)
        .FirstOrDefaultAsync(u => u.Id == id);
}
