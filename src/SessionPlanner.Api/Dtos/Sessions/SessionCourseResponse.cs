namespace SessionPlanner.Api.Dtos.Sessions;

public record SessionCourseResponse(
    int Id,
    string Code,
    string? Name
);
