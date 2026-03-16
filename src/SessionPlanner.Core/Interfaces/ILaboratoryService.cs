using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public enum AddWorkstationStatus
{
    Success,
    LaboratoryNotFound,
    OperatingSystemNotFound
}

public record AddWorkstationResult(AddWorkstationStatus Status, Workstation? Workstation);

public interface ILaboratoryService
{
    Task<List<Laboratory>> GetAllAsync(string? building, int? minCapacity, int? maxCapacity);
    Task<Laboratory?> GetByIdAsync(int id);
    Task<Laboratory> CreateAsync(string name, string building, int numberOfPCs, int seatingCapacity);
    Task<bool> UpdateAsync(int id, string name, string building, int numberOfPCs, int seatingCapacity);
    Task<bool> DeleteAsync(int id);
    Task<AddWorkstationResult> AddWorkstationAsync(int laboratoryId, string name, int osId);
    Task<bool> RemoveWorkstationAsync(int laboratoryId, int workstationId);
}