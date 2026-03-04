using SessionPlanner.Core.Entities.Joins;
namespace SessionPlanner.Core.Entities;

public class EquipmentModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;            // Ex: "Meta Quest 3"
    public int Quantity { get; set; }                    // Column "Quantité"
    public string? DefaultAccessories { get; set; }      // Ex: "Mousse protectrice, câbles, manettes"
    public string? Notes { get; set; }

    public ICollection<CourseEquipmentModel> CourseEquipmentModels { get; set; } = new List<CourseEquipmentModel>();
}