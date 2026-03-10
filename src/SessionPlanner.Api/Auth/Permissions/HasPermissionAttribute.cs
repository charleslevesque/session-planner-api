using Microsoft.AspNetCore.Authorization;

namespace SessionPlanner.Api.Auth;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = permission;
    }
}