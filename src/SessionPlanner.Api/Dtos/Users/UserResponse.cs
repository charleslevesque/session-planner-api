namespace SessionPlanner.Api.Dtos.Users;

public record UserResponse(
    int Id,
    string Username,
    string Roles,
    bool IsActive
);