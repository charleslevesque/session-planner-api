using Microsoft.AspNetCore.Identity;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Entities.Joins;

public class AppUserRole : IdentityUserRole<int>
{
    public virtual AppRole Role { get; set; } = null!;
}
