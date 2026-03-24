using SessionPlanner.Api.Dtos.Softwares;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Softwares;

public sealed class UpdateSoftwareRequestExample : IExamplesProvider<UpdateSoftwareRequest>
{
    public UpdateSoftwareRequest GetExamples()
    {
        return new UpdateSoftwareRequest(
            Name: "Visual Studio 2022 Professional"
        );
    }
}