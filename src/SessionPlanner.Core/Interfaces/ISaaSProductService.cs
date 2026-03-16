using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface ISaaSProductService
{
    Task<SaaSProduct> CreateAsync(string name, int? numberOfAccounts, string? notes);
    Task<List<SaaSProduct>> GetAllAsync();
    Task<SaaSProduct?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, string name, int? numberOfAccounts, string? notes);
    Task<bool> DeleteAsync(int id);
}