using SessionPlanner.Api.Dtos.Softwares;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class SoftwareMappings
{
    public static SoftwareResponse ToResponse(this Software software)
        => new(software.Id, software.Name);
}