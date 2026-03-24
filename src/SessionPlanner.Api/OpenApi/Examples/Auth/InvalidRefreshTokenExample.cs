using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Common;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class InvalidRefreshTokenExample : IExamplesProvider<ApiErrorResponse>
{
    public ApiErrorResponse GetExamples()
    {
        return new ApiErrorResponse(
            Error: "Invalid or expired refresh token.",
            Code: ErrorCodes.InvalidRefreshToken,
            Details: "The refresh token is either invalid, expired, or has already been revoked."
        );
    }
}