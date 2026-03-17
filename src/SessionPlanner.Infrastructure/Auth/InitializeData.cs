using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Infrastructure.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace SessionPlanner.Infrastructure.Data;

public static class InitializeData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        
        await SeedRolesAsync(db);
        await SeedAdminUserAsync(db, passwordService);
        await BackfillUserPersonnelLinksAsync(db);
    }

    private static async Task SeedRolesAsync(AppDbContext db)
    {
        var definitions = RolePermissionDefinitions.Get();

        foreach (var roleDefinition in definitions)
        {
            var roleName = roleDefinition.Key;
            var permissions = roleDefinition.Value.Distinct().ToList();

            var role = await db.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Name == roleName);

            if (role is null)
            {
                role = new Role
                {
                    Name = roleName
                };

                db.Roles.Add(role);
                await db.SaveChangesAsync();
            }

            var existingPermissions = role.RolePermissions
                .Select(rp => rp.Permission)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingPermissions = permissions
                .Where(p => !existingPermissions.Contains(p))
                .ToList();

            foreach (var permission in missingPermissions)
            {
                role.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    Permission = permission
                });
            }

            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedAdminUserAsync(AppDbContext db, IPasswordService passwordService)
    {
        const string adminUsername = "admin@local.dev";
        const string adminPassword = "Password123!";

        var adminUser = await db.Users
            .Include(u => u.UserRoles)
            .SingleOrDefaultAsync(u => u.Username == adminUsername);

        if (adminUser is null)
        {
            adminUser = new User
            {
                Username = adminUsername,
                IsActive = true
            };

            adminUser.PasswordHash = passwordService.HashPassword(adminUser, adminPassword);

            db.Users.Add(adminUser);
            await db.SaveChangesAsync();
        }

        var adminRole = await db.Roles
            .SingleOrDefaultAsync(r => r.Name == Roles.Admin);

        if (adminRole is null)
            throw new InvalidOperationException("Admin role was not found during seeding.");

        var alreadyLinked = await db.UserRoles.AnyAsync(ur =>
            ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);

        if (!alreadyLinked)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });

            await db.SaveChangesAsync();
        }
    }

    private static async Task BackfillUserPersonnelLinksAsync(AppDbContext db)
    {
        var usersWithoutPersonnel = await db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive && u.PersonnelId == null)
            .ToListAsync();

        if (usersWithoutPersonnel.Count == 0)
            return;

        foreach (var user in usersWithoutPersonnel)
        {
            var normalizedEmail = NormalizeEmail(user.Username, user.Id);

            var personnel = await db.Personnel
                .FirstOrDefaultAsync(p => p.Email == normalizedEmail);

            if (personnel is null)
            {
                var localPart = normalizedEmail.Split('@')[0];
                var nameParts = localPart
                    .Split(new[] { '.', '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                personnel = new Personnel
                {
                    FirstName = nameParts.Length > 0 ? ToTitle(nameParts[0]) : "User",
                    LastName = nameParts.Length > 1 ? ToTitle(nameParts[1]) : "Account",
                    Email = normalizedEmail,
                    Function = MapRoleToPersonnelFunction(user.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault()),
                };

                db.Personnel.Add(personnel);
                await db.SaveChangesAsync();
            }

            user.PersonnelId = personnel.Id;
        }

        await db.SaveChangesAsync();
    }

    private static PersonnelFunction MapRoleToPersonnelFunction(string? roleName)
    {
        return roleName switch
        {
            Roles.Teacher => PersonnelFunction.Professor,
            Roles.Technician => PersonnelFunction.LabInstructor,
            _ => PersonnelFunction.CourseInstructor,
        };
    }

    private static string NormalizeEmail(string username, int userId)
    {
        var value = (username ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
            return $"user.{userId}@local.dev";

        return value.Contains('@')
            ? value
            : $"{value}.{userId}@local.dev";
    }

    private static string ToTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "User";

        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }
}