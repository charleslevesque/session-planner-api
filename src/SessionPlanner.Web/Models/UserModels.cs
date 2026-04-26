namespace SessionPlanner.Web.Models;

public record UserResponse(int Id, string Email, string Name, string Role, string CreatedAt);
public record CreateUserRequest(string Email, string Password, string FirstName, string LastName, string Role);
public record UpdateUserRoleRequest(string RoleName);
public record UpdateUserPasswordRequest(string NewPassword);
public record UserActivityResponse(int UserId, string UserName, List<UserActivityItem> Activities);
public record UserActivityItem(string Action, string Details, string Timestamp);
