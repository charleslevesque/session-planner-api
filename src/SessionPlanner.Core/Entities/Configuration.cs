using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

public class Configuration
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = null!;  // Ex: "Ouverture du port 8888"

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Joins
    public ICollection<CourseConfiguration> CourseConfigurations { get; set; } = new List<CourseConfiguration>();
    public ICollection<LaboratoryConfiguration> LaboratoryConfigurations { get; set; } = new List<LaboratoryConfiguration>();
    public ICollection<ConfigurationOS> ConfigurationOSes { get; set; } = new List<ConfigurationOS>();

}