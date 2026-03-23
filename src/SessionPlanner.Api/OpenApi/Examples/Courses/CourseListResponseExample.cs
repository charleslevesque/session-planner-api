using SessionPlanner.Api.Dtos.Courses;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Courses;

public sealed class CourseListResponseExample : IExamplesProvider<IEnumerable<CourseResponse>>
{
    public IEnumerable<CourseResponse> GetExamples()
    {
        return
        [
            new CourseResponse(
                Id: 1,
                Code: "LOG121",
                Name: "Conception orientee objet"
            ),
            new CourseResponse(
                Id: 2,
                Code: "GTI350",
                Name: "Conception et evaluation des interfaces utilisateurs"
            )
        ];
    }
}