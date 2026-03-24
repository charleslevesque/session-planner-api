using SessionPlanner.Api.Dtos.Courses;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Courses;

public sealed class CreateCourseRequestExample : IExamplesProvider<CreateCourseRequest>
{
    public CreateCourseRequest GetExamples()
    {
        return new CreateCourseRequest(
            Code: "LOG121",
            Name: "Conception orientee objet"
        );
    }
}