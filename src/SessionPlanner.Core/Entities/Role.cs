using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Role
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}