namespace SessionPlanner.Api.Dtos.CourseResources;

public record CourseServerResponse(
    int Id,
    string Hostname,
    int CpuCores,
    int RamGb,
    int StorageGb,
    string AccessType,
    string OSName,
    string? Notes
);
