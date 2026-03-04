using SessionPlanner.Core.Entities.Joins;
namespace SessionPlanner.Core.Entities;

public class PhysicalServer
{
    public int Id { get; set; }
    public string Hostname { get; set; } = null!;  // Ex: "atreides.logti.etsmtl.ca" (UNIQUE)
    public int CpuCores { get; set; }
    public int RamGb { get; set; }
    public int StorageGb { get; set; }
    public string AccessType { get; set; } = "Team";
    public string? Notes { get; set; }

    // FK to OS
    public int OSId { get; set; }
    public OS OS { get; set; } = null!;

    // VMs hosted on this server
    public ICollection<VirtualMachine> HostedVirtualMachines { get; set; } = new List<VirtualMachine>();

    // Joins
    public ICollection<CoursePhysicalServer> CoursePhysicalServers { get; set; } = new List<CoursePhysicalServer>();
    public ICollection<PhysicalServerSoftware> PhysicalServerSoftwares { get; set; } = new List<PhysicalServerSoftware>();
    public ICollection<PhysicalServerConfiguration> PhysicalServerConfigurations { get; set; } = new List<PhysicalServerConfiguration>();

}