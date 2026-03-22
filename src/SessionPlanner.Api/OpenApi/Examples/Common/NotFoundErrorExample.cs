using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Common;

public sealed class NotFoundErrorExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples() =>
        new(
            Error: "The requested resource was not found.",
            Code: ErrorCodes.NotFound
        );
}