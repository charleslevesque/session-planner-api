using SessionPlanner.Core.Entities.Joins;
namespace SessionPlanner.Core.Entities;

public enum PersonnelFunction
{
    Professor = 1, 
    LabInstructor = 2, 
    CourseInstructor = 3
}

public class Personnel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public PersonnelFunction Function { get; set; }
    public string Email { get; set; } = null!;  // UNIQUE

    public ICollection<CoursePersonnel> CoursePersonnels { get; set; } = new List<CoursePersonnel>();
}
