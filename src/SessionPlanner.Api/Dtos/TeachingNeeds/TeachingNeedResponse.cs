namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record TeachingNeedResponse(
    int Id,
    int SessionId,
    int PersonnelId,
    string PersonnelFullName,
    int CourseId,
    string CourseCode,
    string? CourseName,
    string Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? ReviewedAt,
    int? ReviewedByUserId,
    string? RejectionReason,
    string? Notes,
    IEnumerable<TeachingNeedItemResponse> Items);
