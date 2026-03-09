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

        await db.Database.MigrateAsync();

        await SeedPermissionsAsync(db);
        await SeedAdminUserAsync(db, passwordService);
    }

    private static async Task SeedPermissionsAsync(AppDbContext db)
    {
        var permissionNames = PermissionHelper.GetAllPermissions(typeof(Permissions));

        foreach (var permissionName in permissionNames)
        {
            if (!await db.Permissions.AnyAsync(p => p.Name == permissionName))
            {
                db.Permissions.Add(new Permission
                {
                    Name = permissionName
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(AppDbContext db, IPasswordService passwordService)
    {
        const string adminUsername = "admin";
        const string adminPassword = "Password123!";

        var existingAdmin = await db.Users
            .Include(u => u.UserPermissions)
            .SingleOrDefaultAsync(u => u.Username == adminUsername);

        if (existingAdmin != null)
            return;

        var adminUser = new User
        {
            Username = adminUsername,
            IsActive = true
        };

        adminUser.PasswordHash = passwordService.HashPassword(adminUser, adminPassword);

        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        var allPermissions = await db.Permissions.ToListAsync();

        foreach (var permission in allPermissions)
        {
            db.UserPermissions.Add(new UserPermission
            {
                UserId = adminUser.Id,
                PermissionId = permission.Id
            });
        }

        await db.SaveChangesAsync();
    }
}