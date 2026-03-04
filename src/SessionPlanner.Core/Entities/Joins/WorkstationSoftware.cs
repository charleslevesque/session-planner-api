using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class WorkstationSoftware
{
    public int WorkstationId { get; set; }
    public Workstation Workstation { get; set; } = null!;
    public int SoftwareId { get; set; }
    public Software Software { get; set; } = null!;
}