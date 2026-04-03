using SessionPlanner.Core.Auth;

namespace SessionPlanner.Infrastructure.Auth;

public static class RolePermissionDefinitions
{
    public static Dictionary<string, List<string>> Get()
    {
        return new Dictionary<string, List<string>>
        {
            [Roles.Admin] = PermissionHelper.GetAllPermissions(typeof(Permissions)).ToList(),

            [Roles.Professor] = new List<string>
            {
                Permissions.Sessions.Read,
                Permissions.TeachingNeeds.Read,
                Permissions.TeachingNeeds.Create,
                Permissions.TeachingNeeds.Update,
                Permissions.TeachingNeeds.Delete,
                Permissions.Courses.Read,
                Permissions.Laboratories.Read,
                Permissions.Workstations.Read,
                Permissions.Softwares.Read,
                Permissions.OperatingSystems.Read,
                Permissions.PhysicalServers.Read,
                Permissions.Personnels.Read,
            },

            [Roles.LabInstructor] = new List<string>
            {
                Permissions.Sessions.Read,
                Permissions.Sessions.Update,
                Permissions.Laboratories.Read,
                Permissions.Laboratories.Create,
                Permissions.Laboratories.Update,
                Permissions.Laboratories.Delete,
                Permissions.Workstations.Read,
                Permissions.Workstations.Create,
                Permissions.Workstations.Update,
                Permissions.Workstations.Delete,
                Permissions.Softwares.Read,
                Permissions.Softwares.Create,
                Permissions.Softwares.Update,
                Permissions.Softwares.Delete,
                Permissions.SoftwareVersions.Read,
                Permissions.SoftwareVersions.Create,
                Permissions.SoftwareVersions.Update,
                Permissions.SoftwareVersions.Delete,
                Permissions.OperatingSystems.Read,
                Permissions.OperatingSystems.Create,
                Permissions.OperatingSystems.Update,
                Permissions.OperatingSystems.Delete,
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

            [Roles.CourseInstructor] = new List<string>
            {
                Permissions.Sessions.Read,
                Permissions.TeachingNeeds.Read,
                Permissions.TeachingNeeds.Create,
                Permissions.TeachingNeeds.Update,
                Permissions.TeachingNeeds.Delete,
                Permissions.Courses.Read,
                Permissions.Laboratories.Read,
                Permissions.Workstations.Read,
                Permissions.Softwares.Read,
                Permissions.OperatingSystems.Read,
                Permissions.PhysicalServers.Read,
                Permissions.Personnels.Read,
            }
        };
    }
}