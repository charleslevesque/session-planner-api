using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public enum CreateUserStatus
{
    Success,
    UsernameAlreadyExists
}

public record CreateUserResult(CreateUserStatus Status, AppUser? User);

public enum UpdateUserRoleStatus
{
    Success,
    UserNotFound,
    RoleNotFound
}

public enum UpdateUserPasswordStatus
{
    Success,
    UserNotFound
}

public enum UpdateCurrentUserEmailStatus
{
    Success,
    UserNotFound,
    InvalidCurrentPassword,
    EmailAlreadyExists,
    ForbiddenForNonAdmin
}

public enum DeactivateUserStatus
{
    Success,
    UserNotFound,
    CannotDeactivateAdmin
}

public enum ReactivateUserStatus
{
    Success,
    UserNotFound,
    AlreadyActive
}

public interface IUserService
{
    Task<List<AppUser>> GetAllActiveWithRolesAsync();
    Task<List<AppUser>> GetAllWithRolesAsync(bool includeInactive = false);
    Task<AppUser?> GetByIdActiveWithRolesAsync(int id);
    Task<AppUser?> GetByIdWithRolesAsync(int id);
    Task<CreateUserResult> CreateAsync(string username, string password, string? roleName);
    Task<UpdateUserRoleStatus> UpdateRoleAsync(int id, string roleName);
    Task<UpdateUserPasswordStatus> UpdatePasswordAsync(int id, string newPassword);
    Task<UpdateCurrentUserEmailStatus> UpdateCurrentUserEmailAsync(int userId, string newEmail, string currentPassword);
    Task<bool> DeleteAsync(int id);
    Task<DeactivateUserStatus> DeactivateAsync(int id);
    Task<ReactivateUserStatus> ReactivateAsync(int id);
    Task<AppUser?> GetByIdWithFullProfileAsync(int id);
}