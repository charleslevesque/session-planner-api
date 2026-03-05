using SessionPlanner.Api.Dtos.Courses;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class CourseMappings
{
    public static CourseResponse ToResponse(this Course course)
    {
        return new CourseResponse(
            course.Id,
            course.Code,
            course.Name
        );
    }
}
