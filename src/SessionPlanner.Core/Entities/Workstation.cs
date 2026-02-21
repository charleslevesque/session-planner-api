namespace SessionPlanner.Core.Entities;

public class Workstation
{
    public int Id { get; set; }

    public int LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; } = null!;

    public int OperatingSystemId { get; set; }
    public OS OperatingSystem { get; set; } = null!;

    public int Count { get; set; }
}
