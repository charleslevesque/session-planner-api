using SessionPlanner.Api.Dtos.Sessions;
using SessionPlanner.Core.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Sessions;

public sealed class SessionListResponseExample : IExamplesProvider<IEnumerable<SessionResponse>>
{
    public IEnumerable<SessionResponse> GetExamples()
    {
        return
        [
            new SessionResponse(
                Id: 1,
                Title: "H2026",
                Status: SessionStatus.Draft,
                StartDate: new DateTime(2026, 1, 05),
                EndDate: new DateTime(2026, 4, 26),
                CreatedAt: DateTime.Now,
                OpenedAt: null,
                ClosedAt: null,
                ArchivedAt: null,
                CreatedByUserId: 2,
                CourseIds: [1, 2]
            ),
            new SessionResponse(
                Id: 2,
                Title: "E2026",
                Status: SessionStatus.Open,
                StartDate: new DateTime(2026, 5, 09),
                EndDate: new DateTime(2026, 9, 29),
                CreatedAt: DateTime.Now,
                OpenedAt: DateTime.Now.AddDays(2),
                ClosedAt: null,
                ArchivedAt: null,
                CreatedByUserId: 1,
                CourseIds: [1, 3, 5]
            )
        ];
    }
}
