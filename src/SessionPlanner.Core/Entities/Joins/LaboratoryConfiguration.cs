using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class LaboratoryConfiguration
{
    public int LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; } = null!;
    public int ConfigurationId { get; set; }
    public Configuration Configuration { get; set; } = null!;
}