using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public enum CreateUserStatus
{
    Success,
    UsernameAlreadyExists
}

public record CreateUserResult(CreateUserStatus Status, User? User);

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

public interface IUserService
{
    Task<List<User>> GetAllActiveWithRolesAsync();
    Task<User?> GetByIdActiveWithRolesAsync(int id);
    Task<User?> GetByIdWithRolesAsync(int id);
    Task<CreateUserResult> CreateAsync(string username, string password, string? roleName);
    Task<UpdateUserRoleStatus> UpdateRoleAsync(int id, string roleName);
    Task<UpdateUserPasswordStatus> UpdatePasswordAsync(int id, string newPassword);
    Task<bool> DeleteAsync(int id);
}