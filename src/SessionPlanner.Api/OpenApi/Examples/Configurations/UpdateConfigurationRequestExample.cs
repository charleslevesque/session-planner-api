using SessionPlanner.Api.Dtos.Configurations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Configurations;

public sealed class UpdateConfigurationRequestExample : IExamplesProvider<UpdateConfigurationRequest>
{
    public UpdateConfigurationRequest GetExamples()
    {
        return new UpdateConfigurationRequest(
            Title: "Software installation (new update)",
            OSIds: [1, 3],
            LaboratoryIds: [1, 2],
            Notes: "REQUIRED MODULES : \nMicrosoft Visual Studio,\nUnity"
        );
    }
}