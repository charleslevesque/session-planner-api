namespace SessionPlanner.Core.Entities;

public class Laboratory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Building { get; set; } = null!;
    public int NumberOfPCs { get; set; }
    public int SeatingCapacity { get; set; }
    
    public ICollection<Workstation> Workstations { get; set; } = new List<Workstation>();
}