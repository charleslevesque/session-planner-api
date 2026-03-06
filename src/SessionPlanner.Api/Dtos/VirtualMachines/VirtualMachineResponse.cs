namespace SessionPlanner.Api.Dtos.VirtualMachines;

public record VirtualMachineResponse(
    int Id,
    int Quantity,
    int CpuCores,
    int RamGb,
    int StorageGb,
    string AccessType,
    string? Notes,
    int OSId,
    string OSName,
    int? HostServerId,
    string? HostServerHostname);
