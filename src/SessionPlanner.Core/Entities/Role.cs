using SessionPlanner.Core.Entities.Joins;
namespace SessionPlanner.Core.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}