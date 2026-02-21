namespace SessionPlanner.Api.Dtos.SoftwareVersions;

public record UpdateSoftwareVersionRequest(
    string VersionNumber,
    string? InstallationDetails,
    string? Notes
);