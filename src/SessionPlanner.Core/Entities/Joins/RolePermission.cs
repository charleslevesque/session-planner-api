using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class RolePermission
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string Permission { get; set; } = null!;
}