using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class InvalidCredentialsExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples() =>
        new(
            Error: "Invalid email or password.",
            Code: "INVALID_CREDENTIALS"
        );
}