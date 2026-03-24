using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Sessions;

public sealed class SessionDeleteNotAllowedExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "This session cannot be deleted in its current state.",
            Code: ErrorCodes.SessionDeleteNotAllowed
        );
    }
}