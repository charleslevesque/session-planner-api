using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Sessions;

public sealed class InvalidSessionTransitionExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "The session cannot be transitioned from its current state.",
            Code: ErrorCodes.InvalidSessionTransition
        );
    }
}