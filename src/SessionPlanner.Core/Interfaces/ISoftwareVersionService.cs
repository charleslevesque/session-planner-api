using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface ISoftwareVersionService
{
    Task<SoftwareVersion?> CreateAsync(int softwareId, int osId, string versionNumber, string? installationDetails, string? notes);
    Task<List<SoftwareVersion>> GetAllAsync();
    Task<List<SoftwareVersion>> GetAllBySoftwareIdAsync(int softwareId);
    Task<SoftwareVersion?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, int osId, string versionNumber, string? installationDetails, string? notes);
    Task<bool> DeleteAsync(int id);
}