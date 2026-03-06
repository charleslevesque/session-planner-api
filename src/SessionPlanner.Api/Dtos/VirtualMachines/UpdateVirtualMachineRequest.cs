namespace SessionPlanner.Api.Dtos.VirtualMachines;

public record UpdateVirtualMachineRequest(
    int Quantity,
    int CpuCores,
    int RamGb,
    int StorageGb,
    string AccessType,
    string? Notes,
    int OSId,
    int? HostServerId);
