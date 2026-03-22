
using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Auth;
using SessionPlanner.Api.Dtos.RefreshTokens;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class RefreshTokenRequestExample : IExamplesProvider<RefreshTokenRequest>
{
    public RefreshTokenRequest GetExamples()
    {
        return new RefreshTokenRequest(
            RefreshToken: "example-refresh-token-value"
        );
    }
}