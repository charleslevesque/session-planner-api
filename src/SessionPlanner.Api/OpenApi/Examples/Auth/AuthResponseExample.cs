using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Auth;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class AuthResponseExample : IExamplesProvider<AuthResponse>
{
    public AuthResponse GetExamples()
    {
        return new AuthResponse(
            Token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.sample",
            RefreshToken: "sample-refresh-token-123",
            ExpiresAt: DateTime.UtcNow.AddMinutes(30)
        );
    }
}