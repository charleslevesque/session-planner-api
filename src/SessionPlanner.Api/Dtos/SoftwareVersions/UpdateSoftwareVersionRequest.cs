namespace SessionPlanner.Api.Dtos.SoftwareVersions;

public record UpdateSoftwareVersionRequest(
    int OsId,
    string VersionNumber,
    string? InstallationDetails,
    string? Notes
);