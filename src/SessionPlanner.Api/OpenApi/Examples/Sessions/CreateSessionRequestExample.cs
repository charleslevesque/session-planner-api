using SessionPlanner.Api.Dtos.Sessions;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Sessions;

public sealed class CreateSessionRequestExample : IExamplesProvider<CreateSessionRequest>
{
    public CreateSessionRequest GetExamples()
    {
        return new CreateSessionRequest(
            Title: "H2026",
            StartDate: new DateTime(2026, 1, 05),
            EndDate: new DateTime(2026, 4, 26),
            CourseIds: [1, 2, 3]
        );
    }
}
