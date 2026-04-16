namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record RenewNeedsResponse(
    TeachingNeedResponse Need,
    IEnumerable<string> Changes
);

public record RenewableCourseResponse(
    int CourseId,
    string CourseCode,
    string? CourseName,
    int SourceNeedId,
    int SourceSessionId,
    string SourceSessionTitle,
    int ItemCount
);
