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
                StartDate: new DateTime(2026, 1, 05),
                EndDate: new DateTime(2026, 4, 26),
                Status: SessionStatus.Draft,
                CreatedAt: DateTime.Now,
                OpenedAt: DateTime.Now.AddDays(3),
                ClosedAt: DateTime.Now.AddDays(6),
                ArchivedAt: DateTime.Now.AddMonths(1),
                CreatedByUserId: 2
            ),
            new SessionResponse(
                Id: 2,
                Title: "E2026",
                StartDate: new DateTime(2026, 5, 09),
                EndDate: new DateTime(2026, 9, 29),
                Status: SessionStatus.Open,
                CreatedAt: DateTime.Now,
                OpenedAt: DateTime.Now.AddDays(2),
                ClosedAt: DateTime.Now.AddDays(15),
                ArchivedAt: DateTime.Now.AddMonths(2),
                CreatedByUserId: 1
            )
        ];
    }
}