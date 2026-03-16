using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public enum VirtualMachineOperationStatus
{
    Success,
    NotFound,
    OperatingSystemNotFound,
    HostServerNotFound
}

public record VirtualMachineOperationResult(VirtualMachineOperationStatus Status, VirtualMachine? VirtualMachine);

public interface IVirtualMachineService
{
    Task<VirtualMachineOperationResult> CreateAsync(
        int quantity,
        int cpuCores,
        int ramGb,
        int storageGb,
        string accessType,
        string? notes,
        int osId,
        int? hostServerId);

    Task<List<VirtualMachine>> GetAllAsync();
    Task<VirtualMachine?> GetByIdAsync(int id);

    Task<VirtualMachineOperationStatus> UpdateAsync(
        int id,
        int quantity,
        int cpuCores,
        int ramGb,
        int storageGb,
        string accessType,
        string? notes,
        int osId,
        int? hostServerId);

    Task<bool> DeleteAsync(int id);
}