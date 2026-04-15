namespace SessionPlanner.Api.Dtos.Users;

public record UserActivityResponse(
    int UserId,
    string Username,
    string? FullName,
    string Role,
    bool IsActive,
    IEnumerable<UserTeachingNeedDetail> TeachingNeeds
);

public record UserTeachingNeedDetail(
    int Id,
    string CourseName,
    string SessionName,
    string Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? ReviewedAt,
    string? RejectionReason,
    string? Notes,
    int? ExpectedStudents,
    string? DesiredModifications,
    string? AdditionalComments,
    bool IsFastTrack,
    IEnumerable<UserTeachingNeedItemDetail> Items
);

public record UserTeachingNeedItemDetail(
    int Id,
    string ItemType,
    string? SoftwareName,
    string? VersionNumber,
    string? OsName,
    int? Quantity,
    string? Description,
    string? Notes
);
