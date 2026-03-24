using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Users;

public sealed class UsernameAlreadyExistsExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "Username already exists.",
            Code: ErrorCodes.Conflict,
            Details: "The username 'teacher01' is already in use."
        );
    }
}