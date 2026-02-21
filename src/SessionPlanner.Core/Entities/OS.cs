namespace SessionPlanner.Core.Entities;

public class OS
{
    public int Id {get; set;}
    public string Name {get; set;} = null!;

    public ICollection<SoftwareVersion> SoftwareVersions {get; set;} = new List<SoftwareVersion>();
}