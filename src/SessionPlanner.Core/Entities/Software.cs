namespace SessionPlanner.Core.Entities;

public class Software
{
    public int ID {get; set;}
    public string Name {get; set;} = null!;

    public ICollection<SoftwareVersion> Versions {get; set;} = new List<SoftwareVersion>();
}