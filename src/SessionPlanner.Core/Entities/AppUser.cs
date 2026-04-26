using Microsoft.AspNetCore.Identity;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

public class AppUser : IdentityUser<int>
{
    public bool IsActive { get; set; } = true;

    public int? PersonnelId { get; set; }
    public Personnel? Personnel { get; set; }

    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
    public ICollection<TeachingNeed> ReviewedTeachingNeeds { get; set; } = new List<TeachingNeed>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
