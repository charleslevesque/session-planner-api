using SessionPlanner.Api.Dtos.OperatingSystems;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.OperatingSystems;

public sealed class OSListResponseExample : IExamplesProvider<IEnumerable<OSResponse>>
{
    public IEnumerable<OSResponse> GetExamples()
    {
        return
        [
            new OSResponse(
                Id: 1,
                Name: "Windows"
            ),
            new OSResponse(
                Id: 2,
                Name: "Linux"
            ),
            new OSResponse(
                Id: 3,
                Name: "macOS"
            )
        ];
    }
}