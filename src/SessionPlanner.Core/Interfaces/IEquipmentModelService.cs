using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface IEquipmentModelService
{
    Task<EquipmentModel> CreateAsync(string name, int quantity, string? defaultAccessories, string? notes);
    Task<List<EquipmentModel>> GetAllAsync();
    Task<EquipmentModel?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, string name, int quantity, string? defaultAccessories, string? notes);
    Task<bool> DeleteAsync(int id);
}