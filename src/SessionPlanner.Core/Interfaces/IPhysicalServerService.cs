using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public enum PhysicalServerOperationStatus
{
    Success,
    NotFound,
    OperatingSystemNotFound,
    DuplicateHostname
}

public record PhysicalServerOperationResult(PhysicalServerOperationStatus Status, PhysicalServer? Server);

public interface IPhysicalServerService
{
    Task<PhysicalServerOperationResult> CreateAsync(
        string hostname,
        int cpuCores,
        int ramGb,
        int storageGb,
        string accessType,
        string? notes,
        int osId);

    Task<List<PhysicalServer>> GetAllAsync();
    Task<PhysicalServer?> GetByIdAsync(int id);

    Task<PhysicalServerOperationStatus> UpdateAsync(
        int id,
        string hostname,
        int cpuCores,
        int ramGb,
        int storageGb,
        string accessType,
        string? notes,
        int osId);

    Task<bool> DeleteAsync(int id);
}