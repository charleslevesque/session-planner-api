using SessionPlanner.Core.Entities.Joins;
namespace SessionPlanner.Core.Entities;

public class Workstation
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;  

    public int LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; } = null!;

    public int OSId { get; set; }  
    public OS OS { get; set; } = null!;

    public ICollection<WorkstationSoftware> WorkstationSoftwares { get; set; } = new List<WorkstationSoftware>();
}