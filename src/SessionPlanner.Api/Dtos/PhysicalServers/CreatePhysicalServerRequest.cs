namespace SessionPlanner.Api.Dtos.PhysicalServers;

public record CreatePhysicalServerRequest(
    string Hostname,
    int CpuCores,
    int RamGb,
    int StorageGb,
    string AccessType,
    string? Notes,
    int OSId);
