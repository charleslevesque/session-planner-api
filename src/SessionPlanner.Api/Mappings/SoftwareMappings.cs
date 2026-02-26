using SessionPlanner.Api.Dtos.Softwares;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class SoftwareMappings
{
    public static SoftwareResponse ToResponse(this Software software)
    {
        return new SoftwareResponse(
            software.Id,
            software.Name,
            software.SoftwareVersions.Select(v => v.ToResponse())
        );
    }
}