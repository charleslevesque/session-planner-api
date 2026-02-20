namespace SessionPlanner.Api.Dtos.SoftwareVersions;

public record SoftwareVersionResponse(
    int Id,
    int SoftwareId,
    int OsId,
    string VersionNumber,
    string? InstallationDetails,
    string? Notes
);