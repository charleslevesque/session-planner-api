using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Common;

public sealed class BadRequestErrorExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples() =>
        new(
            Error: "The request is invalid.",
            Code: ErrorCodes.BadRequest
        );
}