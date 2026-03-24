using SessionPlanner.Api.Dtos.Courses;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Courses;

public sealed class UpdateCourseRequestExample : IExamplesProvider<UpdateCourseRequest>
{
    public UpdateCourseRequest GetExamples()
    {
        return new UpdateCourseRequest(
            Code: "LOG121",
            Name: "Conception orientee objet (new update)"
        );
    }
}