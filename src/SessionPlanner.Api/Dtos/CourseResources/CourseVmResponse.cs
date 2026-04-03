namespace SessionPlanner.Api.Dtos.CourseResources;

public record CourseVmResponse(
    int Id,
    int Quantity,
    int CpuCores,
    int RamGb,
    int StorageGb,
    string AccessType,
    string OSName,
    string? HostServerHostname,
    string? Notes
);
