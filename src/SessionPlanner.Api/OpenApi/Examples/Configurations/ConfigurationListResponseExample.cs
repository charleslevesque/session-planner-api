using SessionPlanner.Api.Dtos.Configurations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Configurations;

public sealed class ConfigurationListResponseExample : IExamplesProvider<IEnumerable<ConfigurationResponse>>
{
    public IEnumerable<ConfigurationResponse> GetExamples()
    {
        return
        [
            new ConfigurationResponse(
                Id: 1,
                Title: "Software installation",
                Notes: "REQUIRED MODULES : \nMicrosoft Visual Studio"
            ),
            new ConfigurationResponse(
                Id: 2,
                Title: "Software installation (new update)",
                Notes: "REQUIRED MODULES : \nMicrosoft Visual Studio,\nUnity"
            )
        ];
    }
}