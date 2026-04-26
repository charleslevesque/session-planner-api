using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Enums;

namespace SessionPlanner.Core.Entities;

public class Personnel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public PersonnelFunction Function { get; set; }
    public string Email { get; set; } = null!;  // UNIQUE

    public User? User { get; set; }

    public ICollection<CoursePersonnel> CoursePersonnels { get; set; } = new List<CoursePersonnel>();
    public ICollection<TeachingNeed> TeachingNeeds { get; set; } = new List<TeachingNeed>();
}
