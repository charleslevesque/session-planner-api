namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record MyNeedResponse(
    int Id,
    int SessionId,
    string SessionTitle,
    int CourseId,
    string CourseCode,
    string? CourseName,
    string Status,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? ReviewedAt,
    string? RejectionReason,
    string? Notes);
