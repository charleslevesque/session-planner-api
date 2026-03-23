using SessionPlanner.Api.Dtos.Laboratories;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Laboratories;

public sealed class AddWorkstationRequestExample : IExamplesProvider<AddWorkstationRequest>
{
    public AddWorkstationRequest GetExamples()
    {
        return new AddWorkstationRequest(
            Name: "WS-B2042-01",
            OSId: 2
        );
    }
}