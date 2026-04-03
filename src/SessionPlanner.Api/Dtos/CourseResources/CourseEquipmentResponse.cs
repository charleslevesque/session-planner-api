namespace SessionPlanner.Api.Dtos.CourseResources;

public record CourseEquipmentResponse(
    int Id,
    string Name,
    int Quantity,
    string? DefaultAccessories,
    string? Notes
);
