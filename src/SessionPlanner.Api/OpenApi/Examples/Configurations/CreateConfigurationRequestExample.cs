using SessionPlanner.Api.Dtos.Configurations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Configurations;

public sealed class CreateConfigurationRequestExample : IExamplesProvider<CreateConfigurationRequest>
{
    public CreateConfigurationRequest GetExamples()
    {
        return new CreateConfigurationRequest(
            Title: "Software installation",
            Notes: "REQUIRED MODULES : \nMicrosoft Visual Studio"

        );
    }
}