namespace SessionPlanner.Api.Dtos.Softwares;

public record SoftwareVersionCatalogEntry(
    int Id,
    string VersionNumber,
    int OsId,
    string OsName,
    string? InstallationDetails,
    string? Notes
);

public record SoftwareCatalogEntry(
    int Id,
    string Name,
    string? InstallCommand,
    IEnumerable<SoftwareVersionCatalogEntry> Versions
);
