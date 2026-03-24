using SessionPlanner.Api.Dtos.SoftwareVersions;

namespace SessionPlanner.Api.Dtos.Softwares;

public record SoftwareResponse(int Id, string Name, string? InstallCommand = null, IEnumerable<SoftwareVersionResponse>? softwareVersions = null);