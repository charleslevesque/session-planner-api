using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.TeachingNeeds;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class RejectTeachingNeedRequestExample : IExamplesProvider<RejectTeachingNeedRequest>
{
    public RejectTeachingNeedRequest GetExamples()
    {
        return new RejectTeachingNeedRequest(
            Reason: "Software version not supported by infrastructure"
        );
    }
}