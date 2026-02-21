using SessionPlanner.Api.Dtos.SoftwareVersions;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class SoftwareVersionMappings
{
    public static SoftwareVersionResponse ToResponse(this SoftwareVersion v)
        => new(v.Id, v.SoftwareId, v.VersionNumber, v.InstallationDetails, v.Notes);
}