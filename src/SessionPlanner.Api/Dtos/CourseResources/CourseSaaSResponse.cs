namespace SessionPlanner.Api.Dtos.CourseResources;

public record CourseSaaSResponse(
    int Id,
    string Name,
    int? NumberOfAccounts,
    string? Notes
);
