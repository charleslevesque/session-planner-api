namespace SessionPlanner.Api.Dtos.CourseResources;

public record CourseConfigurationResponse(
    int Id,
    string Title,
    string? Notes
);
