using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface IOperatingSystemService
{
    Task<List<OS>> GetAllAsync();
    Task<OS?> GetByIdAsync(int id);
    Task<OS> CreateAsync(string name);
    Task<bool> UpdateAsync(int id, string name);
    Task<bool> DeleteAsync(int id);
}