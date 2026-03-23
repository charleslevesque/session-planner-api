using SessionPlanner.Api.Dtos.Users;
using SessionPlanner.Core.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Users;

public sealed class UpdateUserRequestExample : IExamplesProvider<UpdateUserRequest>
{
    public UpdateUserRequest GetExamples()
    {
        return new UpdateUserRequest(
            RoleName: Roles.Technician
        );
    }
}