using SessionPlanner.Core.Entities;
namespace SessionPlanner.Core.Entities.Joins;

public class CourseConfiguration
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int ConfigurationId { get; set; }
    public Configuration Configuration { get; set; } = null!;
}