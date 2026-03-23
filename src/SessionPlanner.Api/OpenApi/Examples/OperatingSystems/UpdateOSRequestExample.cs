using SessionPlanner.Api.Dtos.OperatingSystems;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.OperatingSystems;

public sealed class UpdateOSRequestExample : IExamplesProvider<UpdateOSRequest>
{
    public UpdateOSRequest GetExamples()
    {
        return new UpdateOSRequest(
            Name: "Windows 11 (new update)"
        );
    }
}