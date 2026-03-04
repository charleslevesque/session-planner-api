using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class CourseSaaSProduct
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int SaaSProductId { get; set; }
    public SaaSProduct SaaSProduct { get; set; } = null!;
}