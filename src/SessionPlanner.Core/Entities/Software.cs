using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

public class Software
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? InstallCommand { get; set; }  // Ex: "-7zip (choco)"

    // Joins
    public ICollection<SoftwareVersion> SoftwareVersions { get; set; } = new List<SoftwareVersion>();
    public ICollection<CourseSoftware> CourseSoftwares { get; set; } = new List<CourseSoftware>();
    public ICollection<LaboratorySoftware> LaboratorySoftwares { get; set; } = new List<LaboratorySoftware>();
}