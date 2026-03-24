using SessionPlanner.Api.Common;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Personnel;

public sealed class DuplicatePersonnelEmailExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "A personnel record with this email already exists.",
            Code: ErrorCodes.EmailAlreadyInUse,
            Details: "The email 'jean.douin.2@example.ca' is already assigned to another personnel record."
        );
    }
}