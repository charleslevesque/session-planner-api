using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.VirtualMachines;

public sealed class HostServerNotFoundForVirtualMachineExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "Host server not found.",
            Code: ErrorCodes.BadRequest,
            Details: "No host server exists with id 999."
        );
    }
}