using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Sessions;

public sealed class InvalidSessionDatesExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "EndDate must be after StartDate.",
            Code: ErrorCodes.InvalidSessionDates
        );
    }
}