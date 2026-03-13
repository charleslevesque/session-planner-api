using SessionPlanner.Core.Auth;

namespace SessionPlanner.Infrastructure.Auth;

public static class RolePermissionDefinitions
{
    public static Dictionary<string, List<string>> Get()
    {
        return new Dictionary<string, List<string>>
        {
            [Roles.Admin] = PermissionHelper.GetAllPermissions(typeof(Permissions)).ToList(),

            [Roles.Planner] = new List<string>
            {
                Permissions.Courses.Read,
                Permissions.Courses.Create,
                Permissions.Courses.Update,
                Permissions.Courses.Delete,  
            },

            [Roles.Technician] = new List<string>
            {
                Permissions.Laboratories.Read,
                Permissions.Laboratories.Update,
                Permissions.Laboratories.Create,
                Permissions.Laboratories.Delete,
                Permissions.OperatingSystems.Read,
                Permissions.OperatingSystems.Create,
                Permissions.OperatingSystems.Update,
                Permissions.OperatingSystems.Delete,
                Permissions.Softwares.Read,
                Permissions.Softwares.Create,
                Permissions.Softwares.Update,
                Permissions.Softwares.Delete,
                Permissions.SoftwareVersions.Read,
                Permissions.SoftwareVersions.Create,
                Permissions.SoftwareVersions.Update,
                Permissions.SoftwareVersions.Delete,
                Permissions.Workstations.Read,
                Permissions.Workstations.Create,
                Permissions.Workstations.Update,
                Permissions.Workstations.Delete,
                Permissions.Configurations.Read,
                Permissions.Configurations.Create,
                Permissions.Configurations.Update,
                Permissions.Configurations.Delete,
                Permissions.TeachingNeeds.Read,
                Permissions.TeachingNeeds.Create,
                Permissions.TeachingNeeds.Update,
                Permissions.TeachingNeeds.Delete,
                Permissions.Personnels.Read,
                Permissions.Personnels.Create,
                Permissions.Personnels.Update,
                Permissions.Personnels.Delete,
            },

            [Roles.Management] = new List<string>
            {
                Permissions.Laboratories.Read,
                Permissions.Workstations.Read,
                Permissions.Softwares.Read,
                Permissions.Courses.Read,
                Permissions.OperatingSystems.Read,
                Permissions.SoftwareVersions.Read,
                Permissions.Laboratories.Read,
                Permissions.Personnels.Read,
                Permissions.Workstations.Read,
                Permissions.Configurations.Read,
            },

            [Roles.Teacher] = new List<string>
            {
                Permissions.Laboratories.Read,
                Permissions.Workstations.Read,
                Permissions.Softwares.Read,
                Permissions.Courses.Read,
                Permissions.OperatingSystems.Read,
                Permissions.SoftwareVersions.Read,
                Permissions.Laboratories.Read,
                Permissions.Personnels.Read,
                Permissions.Workstations.Read,
                Permissions.TeachingNeeds.Read,
                Permissions.TeachingNeeds.Create,
                Permissions.TeachingNeeds.Update,
                Permissions.TeachingNeeds.Delete,
            }
        };
    }
}