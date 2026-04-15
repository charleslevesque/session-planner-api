namespace SessionPlanner.Api.Dtos.Users;

public record UserActivityResponse(
    int UserId,
    string Username,
    string? FullName,
    string Role,
    bool IsActive,
    IEnumerable<UserTeachingNeedSummary> TeachingNeeds
);

public record UserTeachingNeedSummary(
    int Id,
    string CourseName,
    string SessionName,
    string Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    int ItemCount
);
