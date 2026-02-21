namespace SessionPlanner.Api.Dtos.SoftwareVersions;

public record CreateSoftwareVersionRequest(
    int SoftwareId,
    string VersionNumber,
    string? InstallationDetails,
    string? Notes
);