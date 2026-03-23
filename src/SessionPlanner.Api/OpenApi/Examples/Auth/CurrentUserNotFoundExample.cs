using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Common;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class CurrentUserNotFoundExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "The current user could not be found.",
            Code: ErrorCodes.NotFound,
            Details: "The user associated with the provided token no longer exists."
        );
    }
}