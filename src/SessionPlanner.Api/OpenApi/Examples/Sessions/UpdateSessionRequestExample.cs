using SessionPlanner.Api.Dtos.Sessions;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Sessions;

public sealed class UpdateSessionRequestExample : IExamplesProvider<UpdateSessionRequest>
{
    public UpdateSessionRequest GetExamples()
    {
        return new UpdateSessionRequest(
            Title: "H2026 (new update)",
            StartDate: new DateTime(2026, 1, 12),
            EndDate: new DateTime(2026, 4, 29)
        );
    }
}