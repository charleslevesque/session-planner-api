using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class VirtualMachineSoftware
{
    public int VirtualMachineId { get; set; }
    public VirtualMachine VirtualMachine { get; set; } = null!;
    public int SoftwareId { get; set; }
    public Software Software { get; set; } = null!;
}