using SessionPlanner.Api.Dtos.Softwares;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Softwares;

public sealed class CreateSoftwareRequestExample : IExamplesProvider<CreateSoftwareRequest>
{
    public CreateSoftwareRequest GetExamples()
    {
        return new CreateSoftwareRequest(
            Name: "Visual Studio 2022"
        );
    }
}