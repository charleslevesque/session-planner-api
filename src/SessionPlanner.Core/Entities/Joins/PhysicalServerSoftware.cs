using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class PhysicalServerSoftware
{
    public int PhysicalServerId { get; set; }
    public PhysicalServer PhysicalServer { get; set; } = null!;
    public int SoftwareId { get; set; }
    public Software Software { get; set; } = null!;
}