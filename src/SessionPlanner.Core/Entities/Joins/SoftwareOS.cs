using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class SoftwareOS
{
    public int SoftwareId { get; set; }
    public Software Software { get; set; } = null!;
    public int OSId { get; set; }
    public OS OS { get; set; } = null!;
}