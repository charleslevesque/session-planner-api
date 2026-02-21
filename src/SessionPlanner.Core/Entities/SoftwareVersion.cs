namespace SessionPlanner.Core.Entities;

public class SoftwareVersion
{
    public int Id {get; set;}
    public string VersionNumber {get; set;} = null!;
    public string? InstallationDetails {get; set;}
    public string? Notes {get; set;}

    public int SoftwareId {get; set;}
    public Software Software {get; set;} = null!;

    public int OsId {get; set;}
    public OS OS {get; set;} = null!;
}