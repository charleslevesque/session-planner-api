using SessionPlanner.Api.Dtos.Courses;
using SessionPlanner.Api.Dtos.CourseResources;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class CourseMappings
{
    public static CourseResponse ToResponse(this Course course)
    {
        return new CourseResponse(
            course.Id,
            course.Code,
            course.Name
        );
    }

    public static CourseSaaSResponse ToCourseSaaSResponse(this SaaSProduct p)
    {
        return new CourseSaaSResponse(p.Id, p.Name, p.NumberOfAccounts, p.Notes);
    }

    public static CourseSoftwareResponse ToCourseSoftwareResponse(this Software s)
    {
        return new CourseSoftwareResponse(s.Id, s.Name, s.InstallCommand);
    }

    public static CourseConfigurationResponse ToCourseConfigurationResponse(this Configuration c)
    {
        var osIds = c.ConfigurationOSes.Select(x => x.OSId).Distinct().ToList();
        var laboratoryIds = c.LaboratoryConfigurations.Select(x => x.LaboratoryId).Distinct().ToList();

        return new CourseConfigurationResponse(c.Id, c.Title, osIds, laboratoryIds, c.Notes);
    }

    public static CourseVmResponse ToCourseVmResponse(this VirtualMachine vm)
    {
        return new CourseVmResponse(
            vm.Id,
            vm.Quantity,
            vm.CpuCores,
            vm.RamGb,
            vm.StorageGb,
            vm.AccessType,
            vm.OS?.Name ?? string.Empty,
            vm.HostServer?.Hostname,
            vm.Notes
        );
    }

    public static CourseServerResponse ToCourseServerResponse(this PhysicalServer ps)
    {
        return new CourseServerResponse(
            ps.Id,
            ps.Hostname,
            ps.CpuCores,
            ps.RamGb,
            ps.StorageGb,
            ps.AccessType,
            ps.OS?.Name ?? string.Empty,
            ps.Notes
        );
    }

    public static CourseEquipmentResponse ToCourseEquipmentResponse(this EquipmentModel e)
    {
        return new CourseEquipmentResponse(e.Id, e.Name, e.Quantity, e.DefaultAccessories, e.Notes);
    }
}
