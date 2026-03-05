using SessionPlanner.Api.Dtos.Configurations;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class ConfigurationMappings
{
    public static ConfigurationResponse ToResponse(this Configuration configuration)
    {
        return new ConfigurationResponse(
            configuration.Id,
            configuration.Title,
            configuration.Notes
        );
    }
}
