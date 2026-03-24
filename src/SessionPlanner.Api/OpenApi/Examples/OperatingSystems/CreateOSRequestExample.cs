using SessionPlanner.Api.Dtos.OperatingSystems;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.OperatingSystems;

public sealed class CreateOSRequestExample : IExamplesProvider<CreateOSRequest>
{
    public CreateOSRequest GetExamples()
    {
        return new CreateOSRequest(
            Name: "Windows"
        );
    }
}