using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

public class Workstation
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public int LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; } = null!;

    public int OSId { get; set; }
    public OS OS { get; set; } = null!;
}