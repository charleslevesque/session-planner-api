using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Common;

public sealed class UnauthorizedErrorExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples() =>
        new(
            Error: "Authentication is required.",
            Code: ErrorCodes.Unauthorized
        );
}