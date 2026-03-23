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
            StartDate: new DateTime(2026, 1, 05),
            EndDate: new DateTime(2026, 4, 26),
            Status: SessionStatus.Draft,
            CreatedAt: DateTime.Now,
            OpenedAt: DateTime.Now.AddDays(3),
            ClosedAt: DateTime.Now.AddDays(6),
            ArchivedAt: DateTime.Now.AddMonths(1),
            CreatedByUserId: 3
        );
    }
}