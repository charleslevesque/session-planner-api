using SessionPlanner.Api.Dtos.Softwares;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Softwares;

public sealed class SoftwareListResponseExample : IExamplesProvider<IEnumerable<SoftwareResponse>>
{
    public IEnumerable<SoftwareResponse> GetExamples()
    {
        return
        [
            new SoftwareResponse(
                Id: 1,
                Name: "Visual Studio 2022"
            ),
            new SoftwareResponse(
                Id: 2,
                Name: "Notepad++"
            )
        ];
    }
}