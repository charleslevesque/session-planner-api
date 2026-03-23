using SessionPlanner.Api.Dtos.OperatingSystems;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.OperatingSystems;

public sealed class OSResponseExample : IExamplesProvider<OSResponse>
{
    public OSResponse GetExamples()
    {
        return new OSResponse(
            Id: 1,
            Name: "Windows 11"
        );
    }
}