using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.TeachingNeeds;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class UpdateTeachingNeedRequestExample : IExamplesProvider<UpdateTeachingNeedRequest>
{
    public UpdateTeachingNeedRequest GetExamples()
    {
        return new UpdateTeachingNeedRequest(
            CourseId: 3,
            Notes: "Updated: added Visual Studio 2022"
        );
    }
}