namespace SessionPlanner.Api.Dtos.Users;

public record CreateUserRequest(
    string Username,
    string Password,
    string RoleName
);