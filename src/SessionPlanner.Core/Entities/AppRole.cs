using Microsoft.AspNetCore.Identity;

namespace SessionPlanner.Core.Entities;

public class AppRole : IdentityRole<int>
{
    public AppRole() { }
    public AppRole(string roleName) : base(roleName) { }
}
