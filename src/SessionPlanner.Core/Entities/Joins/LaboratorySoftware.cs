using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Entities.Joins;

public class LaboratorySoftware
{
    public int LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; } = null!;
    public int SoftwareId { get; set; }
    public Software Software { get; set; } = null!;

    [MaxLength(50)]
    public string Status { get; set; } = null!;
}
