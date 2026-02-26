using SessionPlanner.Api.Dtos.SoftwareVersions;

namespace SessionPlanner.Api.Dtos.OperatingSystems;

public record OSResponse(
    int Id,
    string Name,
    IEnumerable<SoftwareVersionResponse> softwareVersions = null
);