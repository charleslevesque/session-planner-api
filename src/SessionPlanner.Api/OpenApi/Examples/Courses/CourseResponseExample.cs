using SessionPlanner.Api.Dtos.Courses;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Courses;

public sealed class CourseResponseExample : IExamplesProvider<CourseResponse>
{
    public CourseResponse GetExamples()
    {
        return new CourseResponse(
            Id: 1,
            Code: "LOG121",
            Name: "Conception orientee objet"
        );
    }
}