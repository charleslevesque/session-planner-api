namespace SessionPlanner.Api.Dtos.CourseResources;

public record CourseSoftwareResponse(
    int Id,
    string Name,
    string? InstallCommand
);
