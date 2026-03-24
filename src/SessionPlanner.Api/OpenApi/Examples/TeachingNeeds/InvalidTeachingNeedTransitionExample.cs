using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Common;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class InvalidTeachingNeedTransitionExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "Cannot approve a teaching need that is not under review.",
            Code: ErrorCodes.InvalidTeachingNeedTransition
        );
    }
}