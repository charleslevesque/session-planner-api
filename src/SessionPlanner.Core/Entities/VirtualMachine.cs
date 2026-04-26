using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

public class VirtualMachine
{
    public int Id { get; set; }
    public int Quantity { get; set; }           // Column "Nombre"
    public int CpuCores { get; set; }           // Column "CPU"
    public int RamGb { get; set; }              // Column "RAM"
    public int StorageGb { get; set; }          // Column "Stockage"

    [MaxLength(50)]
    public string AccessType { get; set; } = "Team";  // "Par équipe", "Individuel", etc.

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // FK to OS
    public int OSId { get; set; }
    public OS OS { get; set; } = null!;

    // FK to host server (nullable)
    public int? HostServerId { get; set; }
    public PhysicalServer? HostServer { get; set; }

    // Joins
    public ICollection<CourseVirtualMachine> CourseVirtualMachines { get; set; } = new List<CourseVirtualMachine>();
}