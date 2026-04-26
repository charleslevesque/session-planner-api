using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Enums;

namespace SessionPlanner.Core.Entities;

public class Personnel
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [MaxLength(100)]
    public string LastName { get; set; } = null!;
    public PersonnelFunction Function { get; set; }

    [MaxLength(255)]
    public string Email { get; set; } = null!;  // UNIQUE

    public AppUser? User { get; set; }

    public ICollection<CoursePersonnel> CoursePersonnels { get; set; } = new List<CoursePersonnel>();
    public ICollection<TeachingNeed> TeachingNeeds { get; set; } = new List<TeachingNeed>();
}
