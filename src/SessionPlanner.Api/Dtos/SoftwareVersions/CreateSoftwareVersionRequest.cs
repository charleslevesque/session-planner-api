namespace SessionPlanner.Api.Dtos.SoftwareVersions;

public record CreateSoftwareVersionRequest(
    int SoftwareId,
    int OsId,
    string VersionNumber,
    string? InstallationDetails,
    string? Notes
    
);