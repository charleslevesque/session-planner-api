using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class ConfigurationOS
{
    public int ConfigurationId { get; set; }
    public Configuration Configuration { get; set; } = null!;
    public int OSId { get; set; }
    public OS OS { get; set; } = null!;
}