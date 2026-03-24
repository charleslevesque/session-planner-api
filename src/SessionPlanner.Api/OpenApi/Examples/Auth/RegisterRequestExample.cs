using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.Auth;

namespace SessionPlanner.Api.OpenApi.Examples.Auth;

public sealed class RegisterRequestExample : IExamplesProvider<RegisterRequest>
{
    public RegisterRequest GetExamples()
    {
        return new RegisterRequest(
            Email: "teacher@etsmtl.ca",
            Password: "P@ssw0rd123!",
            FirstName: "ExampleFirstName",
            LastName: "ExampleLastName"
        );
    }
}