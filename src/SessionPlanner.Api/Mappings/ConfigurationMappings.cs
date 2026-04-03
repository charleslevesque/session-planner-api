using SessionPlanner.Api.Dtos.Configurations;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class ConfigurationMappings
{
    public static ConfigurationResponse ToResponse(this Configuration configuration)
    {
        var osIds = configuration.ConfigurationOSes.Select(x => x.OSId).Distinct().ToList();
        var laboratoryIds = configuration.LaboratoryConfigurations.Select(x => x.LaboratoryId).Distinct().ToList();

        return new ConfigurationResponse(
            configuration.Id,
            configuration.Title,
            osIds,
            laboratoryIds,
            configuration.Notes
        );
    }
}
