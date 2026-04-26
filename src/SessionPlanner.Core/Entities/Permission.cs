using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Core.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Permission
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}