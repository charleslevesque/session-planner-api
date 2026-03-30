using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface ICourseService
{
    Task<Course> CreateAsync(string code, string? name);
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, string code, string? name);
    Task<bool> DeleteAsync(int id);

    Task<bool> ExistsAsync(int id);
    Task<List<SaaSProduct>> GetCourseSaaSProductsAsync(int courseId);
    Task<List<Software>> GetCourseSoftwaresAsync(int courseId);
    Task<List<Configuration>> GetCourseConfigurationsAsync(int courseId);
    Task<List<VirtualMachine>> GetCourseVirtualMachinesAsync(int courseId);
    Task<List<PhysicalServer>> GetCoursePhysicalServersAsync(int courseId);
    Task<List<EquipmentModel>> GetCourseEquipmentModelsAsync(int courseId);

    Task<bool?> AssociateSaaSProductAsync(int courseId, int saasProductId);
    Task<bool> DissociateSaaSProductAsync(int courseId, int saasProductId);

    Task<bool?> AssociateSoftwareAsync(int courseId, int softwareId);
    Task<bool> DissociateSoftwareAsync(int courseId, int softwareId);

    Task<bool?> AssociateConfigurationAsync(int courseId, int configurationId);
    Task<bool> DissociateConfigurationAsync(int courseId, int configurationId);

    Task<bool?> AssociateVirtualMachineAsync(int courseId, int virtualMachineId);
    Task<bool> DissociateVirtualMachineAsync(int courseId, int virtualMachineId);

    Task<bool?> AssociatePhysicalServerAsync(int courseId, int physicalServerId);
    Task<bool> DissociatePhysicalServerAsync(int courseId, int physicalServerId);

    Task<bool?> AssociateEquipmentModelAsync(int courseId, int equipmentModelId);
    Task<bool> DissociateEquipmentModelAsync(int courseId, int equipmentModelId);

    Task<bool?> AssociateSoftwareVersionAsync(int courseId, int softwareVersionId);
    Task<bool> DissociateSoftwareVersionAsync(int courseId, int softwareVersionId);
    Task<List<int>> GetCourseSoftwareVersionIdsAsync(int courseId);
}