using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class CourseEquipmentModel
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int EquipmentModelId { get; set; }
    public EquipmentModel EquipmentModel { get; set; } = null!;
}