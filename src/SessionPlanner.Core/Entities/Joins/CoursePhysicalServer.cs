using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class CoursePhysicalServer
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int PhysicalServerId { get; set; }
    public PhysicalServer PhysicalServer { get; set; } = null!;
}
