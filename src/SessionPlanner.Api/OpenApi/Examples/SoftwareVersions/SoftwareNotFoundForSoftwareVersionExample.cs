using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.SoftwareVersions;

public sealed class SoftwareNotFoundForSoftwareVersionExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "Software not found.",
            Code: ErrorCodes.BadRequest,
            Details: "Software 999 does not exist."
        );
    }
}