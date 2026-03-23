using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.TeachingNeeds;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class CreateTeachingNeedRequestExample : IExamplesProvider<CreateTeachingNeedRequest>
{
    public CreateTeachingNeedRequest GetExamples()
    {
        return new CreateTeachingNeedRequest(
            PersonnelId: 12,
            CourseId: 3,
            Notes: "Need software for API testing"
        );
    }
}