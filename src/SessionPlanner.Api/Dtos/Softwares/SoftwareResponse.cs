using SessionPlanner.Api.Dtos.SoftwareVersions;

namespace SessionPlanner.Api.Dtos.Softwares;

public record SoftwareResponse(int Id, string Name, IEnumerable<SoftwareVersionResponse> softwareVersions = null);
//public record SoftwareResponse(int Id, string Name);