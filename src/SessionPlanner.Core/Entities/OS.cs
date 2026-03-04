using SessionPlanner.Core.Entities.Joins;
namespace SessionPlanner.Core.Entities;public class OS

{
    public int Id {get; set;}
    public string Name {get; set;} = null!;

    public ICollection<SoftwareVersion> SoftwareVersions {get; set;} = new List<SoftwareVersion>();
    public ICollection<Workstation> Workstations { get; set; } = new List<Workstation>();
    public ICollection<VirtualMachine> VirtualMachines { get; set; } = new List<VirtualMachine>();
    public ICollection<PhysicalServer> PhysicalServers { get; set; } = new List<PhysicalServer>();
    public ICollection<SoftwareOS> SoftwareOSes { get; set; } = new List<SoftwareOS>();
    public ICollection<ConfigurationOS> ConfigurationOSes { get; set; } = new List<ConfigurationOS>();
}