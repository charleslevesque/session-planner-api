using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Entities.Joins;

public class SessionCourse
{
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
