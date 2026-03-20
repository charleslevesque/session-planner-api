using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Interfaces;

public interface ILaboratorySoftwareService
{
    Task<List<LaboratorySoftware>> GetAllAsync();
    Task<List<LaboratorySoftware>> GetByLaboratoryAsync(int laboratoryId);
    Task<LaboratorySoftware?> UpsertAsync(int laboratoryId, int softwareId, string status);
    Task<bool> DeleteAsync(int laboratoryId, int softwareId);
}
