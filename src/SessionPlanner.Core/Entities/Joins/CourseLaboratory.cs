using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class CourseLaboratory
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; } = null!;
}