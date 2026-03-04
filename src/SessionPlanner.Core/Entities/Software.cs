using SessionPlanner.Core.Entities.Joins;
namespace SessionPlanner.Core.Entities;

public class Software
{
    public int Id {get; set;}
    public string Name {get; set;} = null!;
    public string? InstallCommand { get; set; }  // Ex: "-7zip (choco)"

    //Joins 
    public ICollection<SoftwareVersion> SoftwareVersions {get; set;} = new List<SoftwareVersion>();
    public ICollection<WorkstationSoftware> WorkstationSoftwares { get; set; } = new List<WorkstationSoftware>();
    public ICollection<CourseSoftware> CourseSoftwares { get; set; } =new List<CourseSoftware>();
    public ICollection<VirtualMachineSoftware> VirtualMachineSoftwares { get; set; } =new List<VirtualMachineSoftware>();
    public ICollection<PhysicalServerSoftware> PhysicalServerSoftwares { get; set; } =new List<PhysicalServerSoftware>();
    public ICollection<SoftwareOS> SoftwareOSes { get; set; } = new List<SoftwareOS>();
}