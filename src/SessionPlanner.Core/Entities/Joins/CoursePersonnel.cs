using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class CoursePersonnel
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int PersonnelId { get; set; }
    public Personnel Personnel { get; set; } = null!;
}