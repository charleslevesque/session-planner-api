using SessionPlanner.Api.Dtos.Sessions;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class SessionMappings
{
    public static SessionResponse ToResponse(this Session session)
    {
        return new SessionResponse(
            session.Id,
            session.Title,
            session.Status,
            session.StartDate,
            session.EndDate,
            session.CreatedAt,
            session.OpenedAt,
            session.ClosedAt,
            session.ArchivedAt,
            session.CreatedByUserId,
            session.SessionCourses?.Select(sc => sc.CourseId).OrderBy(id => id).ToList()
        );
    }

    public static SessionCourseResponse ToCourseResponse(this Course course)
    {
        return new SessionCourseResponse(course.Id, course.Code, course.Name);
    }
}
