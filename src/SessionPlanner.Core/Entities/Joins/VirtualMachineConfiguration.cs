using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class VirtualMachineConfiguration
{
    public int VirtualMachineId { get; set; }
    public VirtualMachine VirtualMachine { get; set; } = null!;
    public int ConfigurationId { get; set; }
    public Configuration Configuration { get; set; } = null!;
}