using SessionPlanner.Api.Dtos.Configurations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Configurations;

public sealed class ConfigurationResponseExample : IExamplesProvider<ConfigurationResponse>
{
    public ConfigurationResponse GetExamples()
    {
        return new ConfigurationResponse(
            Id: 1,
            Title: "Software installation",
            OSIds: [1, 2],
            LaboratoryIds: [1, 2],
            Notes: "REQUIRED MODULES : \nMicrosoft Visual Studio"
        );
    }
}