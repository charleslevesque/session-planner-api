using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.PhysicalServers;

public sealed class DuplicatePhysicalServerHostnameExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "A server with this hostname already exists.",
            Code: ErrorCodes.Conflict,
            Details: "The hostname 'examplesrv.example.ca' is already assigned to another physical server."
        );
    }
}