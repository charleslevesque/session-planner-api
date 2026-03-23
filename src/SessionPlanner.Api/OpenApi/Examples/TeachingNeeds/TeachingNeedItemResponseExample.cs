using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.TeachingNeeds;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class TeachingNeedItemExample : IExamplesProvider<TeachingNeedItemResponse>
{
    public TeachingNeedItemResponse GetExamples()
    {
        return new TeachingNeedItemResponse(
            Id: 1,
            SoftwareId: 10,
            SoftwareVersionId: 25,
            SoftwareVersionNumber: "2022.1.4",
            SoftwareName: "Photoshop CC",
            OSId: 2,
            OSName: "Windows",
            Quantity: 30,
            Notes: "Installed on all machines"
        );
    }
}