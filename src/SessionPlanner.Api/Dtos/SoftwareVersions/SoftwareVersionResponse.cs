namespace SessionPlanner.Api.Dtos.SoftwareVersions;

public record SoftwareVersionResponse(
    int Id,
    int SoftwareId,
    string VersionNumber,
    string? InstallationDetails,
    string? Notes
);