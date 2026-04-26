using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

public class Laboratory
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(100)]
    public string Building { get; set; } = null!;
    public int NumberOfPCs { get; set; }
    public int SeatingCapacity { get; set; }

    public ICollection<Workstation> Workstations { get; set; } = new List<Workstation>();
    // Joins
    public ICollection<CourseLaboratory> CourseLaboratories { get; set; } = new List<CourseLaboratory>();
    public ICollection<LaboratoryConfiguration> LaboratoryConfigurations { get; set; } = new List<LaboratoryConfiguration>();
    public ICollection<LaboratorySoftware> LaboratorySoftwares { get; set; } = new List<LaboratorySoftware>();
}