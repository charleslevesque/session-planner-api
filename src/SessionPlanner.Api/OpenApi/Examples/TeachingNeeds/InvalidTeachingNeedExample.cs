using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Common;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class InvalidTeachingNeedExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "PersonnelId is required.",
            Code: ErrorCodes.BadRequest
        );
    }
}