using SessionPlanner.Api.Dtos.Sessions;
using SessionPlanner.Core.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Sessions;

public sealed class SessionResponseExample : IExamplesProvider<SessionResponse>
{
    public SessionResponse GetExamples()
    {
        return new SessionResponse(
            Id: 1,
            Title: "H2026",
            Status: SessionStatus.Draft,
            StartDate: new DateTime(2026, 1, 05),
            EndDate: new DateTime(2026, 4, 26),
            CreatedAt: DateTime.Now,
            OpenedAt: null,
            ClosedAt: null,
            ArchivedAt: null,
            CreatedByUserId: 3,
            CourseIds: [1, 2, 3]
        );
    }
}
