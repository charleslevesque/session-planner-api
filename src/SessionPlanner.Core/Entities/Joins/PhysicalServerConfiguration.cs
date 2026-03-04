using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class PhysicalServerConfiguration
{
    public int PhysicalServerId { get; set; }
    public PhysicalServer PhysicalServer { get; set; } = null!;
    public int ConfigurationId { get; set; }
    public Configuration Configuration { get; set; } = null!;
}