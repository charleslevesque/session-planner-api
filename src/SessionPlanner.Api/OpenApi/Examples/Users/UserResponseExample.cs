using SessionPlanner.Api.Dtos.Users;
using SessionPlanner.Core.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Users;

public sealed class UserResponseExample : IExamplesProvider<UserResponse>
{
    public UserResponse GetExamples()
    {
        return new UserResponse(
            Id: 1,
            Username: "teacher01",
            Roles: Roles.Teacher
        );
    }
}