using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Common;

public sealed class ForbiddenErrorExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples() =>
        new(
            Error: "You do not have permission to perform this action.",
            Code: ErrorCodes.Forbidden
        );
}