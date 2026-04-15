using SessionPlanner.Api.Dtos.Users;
using SessionPlanner.Core.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Users;

public sealed class UserListResponseExample : IExamplesProvider<IEnumerable<UserResponse>>
{
    public IEnumerable<UserResponse> GetExamples()
    {
        return
        [
            new UserResponse(
                Id: 1,
                Username: "teacher01",
                Roles: Roles.Admin,
                IsActive: true
            ),
            new UserResponse(
                Id: 2,
                Username: "admin01",
                Roles: Roles.Admin,
                IsActive: true
            )
        ];
    }
}