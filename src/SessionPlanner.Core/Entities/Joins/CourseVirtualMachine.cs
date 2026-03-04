using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class CourseVirtualMachine
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int VirtualMachineId { get; set; }
    public VirtualMachine VirtualMachine { get; set; } = null!;
}