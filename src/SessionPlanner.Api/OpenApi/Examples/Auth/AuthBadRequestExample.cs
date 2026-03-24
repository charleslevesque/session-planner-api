using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Common;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class AuthBadRequestExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "An account with this email already exists."
        );
    }
}