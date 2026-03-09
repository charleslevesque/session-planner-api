namespace SessionPlanner.Core.Entities.Joins;

public class UserPermission
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}