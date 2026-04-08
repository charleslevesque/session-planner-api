using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface ISoftwareService
{
    Task<Software> CreateAsync(string name);
    Task<List<Software>> GetAllAsync();
    Task<Software?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, string name);
    Task<bool> DeleteAsync(int id);

    /// <summary>Returns all softwares that have at least one version, with their versions and OS info, ordered by name.</summary>
    Task<List<Software>> GetCatalogAsync();
}