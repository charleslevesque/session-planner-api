using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Auth;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest(
            Email: "teacher@etsmtl.ca",
            Password: "P@ssw0rd123!"
        );
    }
}