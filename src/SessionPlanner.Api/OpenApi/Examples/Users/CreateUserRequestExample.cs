using SessionPlanner.Api.Dtos.Users;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Users;

public sealed class CreateUserRequestExample : IExamplesProvider<CreateUserRequest>
{
    public CreateUserRequest GetExamples()
    {
        return new CreateUserRequest(
            Username: "teacher01",
            Password: "P@ssw0rd123!",
            RoleName: Roles.Teacher
        );
    }
}