using SessionPlanner.Api.Dtos.Softwares;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Softwares;

public sealed class SoftwareResponseExample : IExamplesProvider<SoftwareResponse>
{
    public SoftwareResponse GetExamples()
    {
        return new SoftwareResponse(
            Id: 1,
            Name: "Visual Studio 2022"
        );
    }
}