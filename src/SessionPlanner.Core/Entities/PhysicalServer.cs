using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

public class PhysicalServer
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Hostname { get; set; } = null!;  // Ex: "atreides.logti.etsmtl.ca" (UNIQUE)
    public int CpuCores { get; set; }
    public int RamGb { get; set; }
    public int StorageGb { get; set; }

    [MaxLength(50)]
    public string AccessType { get; set; } = "Team";

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // FK to OS
    public int OSId { get; set; }
    public OS OS { get; set; } = null!;

    // VMs hosted on this server
    public ICollection<VirtualMachine> HostedVirtualMachines { get; set; } = new List<VirtualMachine>();

    // Joins
    public ICollection<CoursePhysicalServer> CoursePhysicalServers { get; set; } = new List<CoursePhysicalServer>();

}