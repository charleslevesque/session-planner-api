using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

public class EquipmentModel
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = null!;            // Ex: "Meta Quest 3"
    public int Quantity { get; set; }                    // Column "Quantité"

    [MaxLength(500)]
    public string? DefaultAccessories { get; set; }      // Ex: "Mousse protectrice, câbles, manettes"

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public ICollection<CourseEquipmentModel> CourseEquipmentModels { get; set; } = new List<CourseEquipmentModel>();
}