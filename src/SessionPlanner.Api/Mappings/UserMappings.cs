using SessionPlanner.Api.Dtos.Users;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class UserMappings
{
    public static UserResponse ToResponse(this AppUser user)
    {
        var roleName = user.UserRoles
            .Select(ur => ur.Role.Name)
            .FirstOrDefault() ?? string.Empty;

        return new UserResponse(
            user.Id,
            user.UserName ?? string.Empty,
            roleName,
            user.IsActive
        );
    }
}