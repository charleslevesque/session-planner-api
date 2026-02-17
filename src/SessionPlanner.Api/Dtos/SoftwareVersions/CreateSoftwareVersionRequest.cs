namespace SessionPlanner.Api.Dtos.SoftwareVersions;

public record CreateSoftwareVersionRequest(
    string VersionNumber,
    string? InstallationDetails,
    string? Notes
);