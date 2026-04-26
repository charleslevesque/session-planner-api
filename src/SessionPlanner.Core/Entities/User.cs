using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

[Index(nameof(Username), IsUnique = true)]
public class User
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? PersonnelId { get; set; }
    public Personnel? Personnel { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<TeachingNeed> ReviewedTeachingNeeds { get; set; } = new List<TeachingNeed>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}