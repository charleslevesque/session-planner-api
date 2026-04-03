using SessionPlanner.Api.Dtos.Configurations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Configurations;

public sealed class CreateConfigurationRequestExample : IExamplesProvider<CreateConfigurationRequest>
{
    public CreateConfigurationRequest GetExamples()
    {
        return new CreateConfigurationRequest(
            Title: "Software installation",
            OSIds: [1, 2],
            LaboratoryIds: [1, 2],
            Notes: "REQUIRED MODULES : \nMicrosoft Visual Studio"

        );
    }
}