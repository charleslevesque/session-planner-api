using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface IConfigurationService
{
    Task<Configuration> CreateAsync(string title, IReadOnlyList<int> osIds, IReadOnlyList<int> laboratoryIds, string? notes);
    Task<List<Configuration>> GetAllAsync();
    Task<Configuration?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, string title, IReadOnlyList<int> osIds, IReadOnlyList<int> laboratoryIds, string? notes);
    Task<bool> DeleteAsync(int id);
}