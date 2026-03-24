using SessionPlanner.Api.Dtos.Configurations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Configurations;

public sealed class UpdateConfigurationRequestExample : IExamplesProvider<UpdateConfigurationRequest>
{
    public UpdateConfigurationRequest GetExamples()
    {
        return new UpdateConfigurationRequest(
            Title: "Software installation (new update)",
            Notes: "REQUIRED MODULES : \nMicrosoft Visual Studio,\nUnity"
        );
    }
}