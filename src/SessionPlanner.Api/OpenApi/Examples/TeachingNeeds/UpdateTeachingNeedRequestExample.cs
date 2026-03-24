using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.TeachingNeeds;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class UpdateTeachingNeedRequestExample : IExamplesProvider<UpdateTeachingNeedRequest>
{
    public UpdateTeachingNeedRequest GetExamples()
    {
        return new UpdateTeachingNeedRequest(
            CourseId: 3,
            ExpectedStudents: 50,
            HasTechNeeds: false,
            FoundAllCourses: false,
            DesiredModifications: "I need the regular license",
            AdditionalComments: "I need more licenses, more students than expected",
            AllowsUpdates: false,
            Notes: ""
        );
    }
}