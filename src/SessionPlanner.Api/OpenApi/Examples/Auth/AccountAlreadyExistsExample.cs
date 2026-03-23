using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Common;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class AccountAlreadyExistsExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "An account with this email already exists.",
            Code: ErrorCodes.Conflict
        );
    }
}