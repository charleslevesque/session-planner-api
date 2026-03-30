namespace SessionPlanner.Core.Entities.Joins;

public class CourseSoftwareVersion
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int SoftwareVersionId { get; set; }
    public SoftwareVersion SoftwareVersion { get; set; } = null!;
}
