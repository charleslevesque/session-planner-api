using SessionPlanner.Core.Entities.Joins;
namespace SessionPlanner.Core.Entities;

public class Course 
{
    public int Id { get; set; }
    public string Code { get; set; } = null!; // Ex: "LOG430"
    public string? Name { get; set; }

    //Joins
    public ICollection<CourseSoftware> CourseSoftwares { get; set; } = new List<CourseSoftware>();
    public ICollection<CourseLaboratory> CourseLaboratories { get; set; } = new List<CourseLaboratory>();
    public ICollection<CourseConfiguration> CourseConfigurations { get; set; } = new List<CourseConfiguration>();
    public ICollection<CourseVirtualMachine> CourseVirtualMachines { get; set; } = new List<CourseVirtualMachine>();
    public ICollection<CoursePhysicalServer> CoursePhysicalServers { get; set; } = new List<CoursePhysicalServer>();
    public ICollection<CourseSaaSProduct> CourseSaaSProducts { get; set; } = new List<CourseSaaSProduct>();
    public ICollection<CourseEquipmentModel> CourseEquipmentModels { get; set; } = new List<CourseEquipmentModel>();
    public ICollection<CoursePersonnel> CoursePersonnels { get; set; } = new List<CoursePersonnel>();
    public ICollection<TeachingNeed> TeachingNeeds { get; set; } = new List<TeachingNeed>();
    public ICollection<SessionCourse> SessionCourses { get; set; } = new List<SessionCourse>();
}