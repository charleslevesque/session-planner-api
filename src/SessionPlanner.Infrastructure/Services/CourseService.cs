using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Course> CreateAsync(string code, string? name)
    {
        var course = new Course
        {
            Code = code,
            Name = name
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return course;
    }

    public async Task<List<Course>> GetAllAsync()
    {
        return await _db.Courses.ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _db.Courses.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(int id, string code, string? name)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course is null)
            return false;

        course.Code = code;
        course.Name = name;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course is null)
            return false;

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _db.Courses.AsNoTracking().AnyAsync(c => c.Id == id);
    }

    public async Task<List<SaaSProduct>> GetCourseSaaSProductsAsync(int courseId)
    {
        return await _db.Set<CourseSaaSProduct>()
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .Select(x => x.SaaSProduct)
            .ToListAsync();
    }

    public async Task<List<Software>> GetCourseSoftwaresAsync(int courseId)
    {
        return await _db.Set<CourseSoftware>()
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .Select(x => x.Software)
            .ToListAsync();
    }

    public async Task<List<Configuration>> GetCourseConfigurationsAsync(int courseId)
    {
        return await _db.Set<CourseConfiguration>()
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .Select(x => x.Configuration)
            .ToListAsync();
    }

    public async Task<List<VirtualMachine>> GetCourseVirtualMachinesAsync(int courseId)
    {
        return await _db.Set<CourseVirtualMachine>()
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .Select(x => new VirtualMachine
            {
                Id = x.VirtualMachine.Id,
                Quantity = x.VirtualMachine.Quantity,
                CpuCores = x.VirtualMachine.CpuCores,
                RamGb = x.VirtualMachine.RamGb,
                StorageGb = x.VirtualMachine.StorageGb,
                AccessType = x.VirtualMachine.AccessType,
                Notes = x.VirtualMachine.Notes,
                OSId = x.VirtualMachine.OSId,
                OS = x.VirtualMachine.OS,
                HostServerId = x.VirtualMachine.HostServerId,
                HostServer = x.VirtualMachine.HostServer
            })
            .ToListAsync();
    }

    public async Task<List<PhysicalServer>> GetCoursePhysicalServersAsync(int courseId)
    {
        return await _db.Set<CoursePhysicalServer>()
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .Select(x => new PhysicalServer
            {
                Id = x.PhysicalServer.Id,
                Hostname = x.PhysicalServer.Hostname,
                CpuCores = x.PhysicalServer.CpuCores,
                RamGb = x.PhysicalServer.RamGb,
                StorageGb = x.PhysicalServer.StorageGb,
                AccessType = x.PhysicalServer.AccessType,
                Notes = x.PhysicalServer.Notes,
                OSId = x.PhysicalServer.OSId,
                OS = x.PhysicalServer.OS
            })
            .ToListAsync();
    }

    public async Task<List<EquipmentModel>> GetCourseEquipmentModelsAsync(int courseId)
    {
        return await _db.Set<CourseEquipmentModel>()
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .Select(x => x.EquipmentModel)
            .ToListAsync();
    }

    // ── Association / Dissociation ──

    public async Task<bool?> AssociateSaaSProductAsync(int courseId, int saasProductId)
    {
        if (!await _db.SaaSProducts.AnyAsync(r => r.Id == saasProductId))
            return null;
        var set = _db.Set<CourseSaaSProduct>();
        if (await set.AnyAsync(j => j.CourseId == courseId && j.SaaSProductId == saasProductId))
            return false;
        set.Add(new CourseSaaSProduct { CourseId = courseId, SaaSProductId = saasProductId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DissociateSaaSProductAsync(int courseId, int saasProductId)
    {
        var join = await _db.Set<CourseSaaSProduct>()
            .FirstOrDefaultAsync(j => j.CourseId == courseId && j.SaaSProductId == saasProductId);
        if (join is null) return false;
        _db.Set<CourseSaaSProduct>().Remove(join);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> AssociateSoftwareAsync(int courseId, int softwareId)
    {
        if (!await _db.Softwares.AnyAsync(r => r.Id == softwareId))
            return null;
        var set = _db.Set<CourseSoftware>();
        if (await set.AnyAsync(j => j.CourseId == courseId && j.SoftwareId == softwareId))
            return false;
        set.Add(new CourseSoftware { CourseId = courseId, SoftwareId = softwareId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DissociateSoftwareAsync(int courseId, int softwareId)
    {
        var join = await _db.Set<CourseSoftware>()
            .FirstOrDefaultAsync(j => j.CourseId == courseId && j.SoftwareId == softwareId);
        if (join is null) return false;
        _db.Set<CourseSoftware>().Remove(join);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> AssociateConfigurationAsync(int courseId, int configurationId)
    {
        if (!await _db.Configurations.AnyAsync(r => r.Id == configurationId))
            return null;
        var set = _db.Set<CourseConfiguration>();
        if (await set.AnyAsync(j => j.CourseId == courseId && j.ConfigurationId == configurationId))
            return false;
        set.Add(new CourseConfiguration { CourseId = courseId, ConfigurationId = configurationId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DissociateConfigurationAsync(int courseId, int configurationId)
    {
        var join = await _db.Set<CourseConfiguration>()
            .FirstOrDefaultAsync(j => j.CourseId == courseId && j.ConfigurationId == configurationId);
        if (join is null) return false;
        _db.Set<CourseConfiguration>().Remove(join);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> AssociateVirtualMachineAsync(int courseId, int virtualMachineId)
    {
        if (!await _db.VirtualMachines.AnyAsync(r => r.Id == virtualMachineId))
            return null;
        var set = _db.Set<CourseVirtualMachine>();
        if (await set.AnyAsync(j => j.CourseId == courseId && j.VirtualMachineId == virtualMachineId))
            return false;
        set.Add(new CourseVirtualMachine { CourseId = courseId, VirtualMachineId = virtualMachineId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DissociateVirtualMachineAsync(int courseId, int virtualMachineId)
    {
        var join = await _db.Set<CourseVirtualMachine>()
            .FirstOrDefaultAsync(j => j.CourseId == courseId && j.VirtualMachineId == virtualMachineId);
        if (join is null) return false;
        _db.Set<CourseVirtualMachine>().Remove(join);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> AssociatePhysicalServerAsync(int courseId, int physicalServerId)
    {
        if (!await _db.PhysicalServers.AnyAsync(r => r.Id == physicalServerId))
            return null;
        var set = _db.Set<CoursePhysicalServer>();
        if (await set.AnyAsync(j => j.CourseId == courseId && j.PhysicalServerId == physicalServerId))
            return false;
        set.Add(new CoursePhysicalServer { CourseId = courseId, PhysicalServerId = physicalServerId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DissociatePhysicalServerAsync(int courseId, int physicalServerId)
    {
        var join = await _db.Set<CoursePhysicalServer>()
            .FirstOrDefaultAsync(j => j.CourseId == courseId && j.PhysicalServerId == physicalServerId);
        if (join is null) return false;
        _db.Set<CoursePhysicalServer>().Remove(join);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> AssociateEquipmentModelAsync(int courseId, int equipmentModelId)
    {
        if (!await _db.EquipmentModels.AnyAsync(r => r.Id == equipmentModelId))
            return null;
        var set = _db.Set<CourseEquipmentModel>();
        if (await set.AnyAsync(j => j.CourseId == courseId && j.EquipmentModelId == equipmentModelId))
            return false;
        set.Add(new CourseEquipmentModel { CourseId = courseId, EquipmentModelId = equipmentModelId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DissociateEquipmentModelAsync(int courseId, int equipmentModelId)
    {
        var join = await _db.Set<CourseEquipmentModel>()
            .FirstOrDefaultAsync(j => j.CourseId == courseId && j.EquipmentModelId == equipmentModelId);
        if (join is null) return false;
        _db.Set<CourseEquipmentModel>().Remove(join);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> AssociateSoftwareVersionAsync(int courseId, int softwareVersionId)
    {
        var version = await _db.SoftwareVersions
            .AsNoTracking()
            .Where(r => r.Id == softwareVersionId)
            .Select(r => new { r.Id, r.SoftwareId })
            .FirstOrDefaultAsync();
        if (version is null)
            return null;

        var csvSet = _db.Set<CourseSoftwareVersion>();
        if (await csvSet.AnyAsync(j => j.CourseId == courseId && j.SoftwareVersionId == softwareVersionId))
            return false;

        csvSet.Add(new CourseSoftwareVersion { CourseId = courseId, SoftwareVersionId = softwareVersionId });

        var csSet = _db.Set<CourseSoftware>();
        if (!await csSet.AnyAsync(j => j.CourseId == courseId && j.SoftwareId == version.SoftwareId))
            csSet.Add(new CourseSoftware { CourseId = courseId, SoftwareId = version.SoftwareId });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DissociateSoftwareVersionAsync(int courseId, int softwareVersionId)
    {
        var join = await _db.Set<CourseSoftwareVersion>()
            .FirstOrDefaultAsync(j => j.CourseId == courseId && j.SoftwareVersionId == softwareVersionId);
        if (join is null) return false;

        var softwareId = await _db.SoftwareVersions
            .Where(sv => sv.Id == softwareVersionId)
            .Select(sv => sv.SoftwareId)
            .FirstOrDefaultAsync();

        _db.Set<CourseSoftwareVersion>().Remove(join);

        var otherVersionsStillLinked = await _db.Set<CourseSoftwareVersion>()
            .AnyAsync(j =>
                j.CourseId == courseId &&
                j.SoftwareVersionId != softwareVersionId &&
                _db.SoftwareVersions.Any(sv => sv.Id == j.SoftwareVersionId && sv.SoftwareId == softwareId));

        if (!otherVersionsStillLinked)
        {
            var csJoin = await _db.Set<CourseSoftware>()
                .FirstOrDefaultAsync(j => j.CourseId == courseId && j.SoftwareId == softwareId);
            if (csJoin is not null)
                _db.Set<CourseSoftware>().Remove(csJoin);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<int>> GetCourseSoftwareVersionIdsAsync(int courseId)
    {
        return await _db.Set<CourseSoftwareVersion>()
            .AsNoTracking()
            .Where(j => j.CourseId == courseId)
            .Select(j => j.SoftwareVersionId)
            .ToListAsync();
    }
}