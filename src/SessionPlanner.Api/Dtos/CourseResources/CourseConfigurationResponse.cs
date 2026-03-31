namespace SessionPlanner.Api.Dtos.CourseResources;

public record CourseConfigurationResponse(
    int Id,
    string Title,
    IReadOnlyList<int> OSIds,
    IReadOnlyList<int> LaboratoryIds,
    string? Notes
);
