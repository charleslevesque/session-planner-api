using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Core.Entities;

public class SoftwareVersion
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string VersionNumber { get; set; } = null!;

    [MaxLength(2000)]
    public string? InstallationDetails { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int SoftwareId { get; set; }
    public Software Software { get; set; } = null!;

    public int OsId { get; set; }
    public OS OS { get; set; } = null!;
}