using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class CourseSoftware
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int SoftwareId { get; set; }
    public Software Software { get; set; } = null!;
}