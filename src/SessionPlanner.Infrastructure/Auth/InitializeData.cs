using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Enums;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Auth;

public static class InitializeData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await BackfillUserPersonnelLinksAsync(userManager, db);
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
    {
        var definitions = RolePermissionDefinitions.Get();

        foreach (var roleDefinition in definitions)
        {
            var roleName = roleDefinition.Key;
            var permissions = roleDefinition.Value.Distinct().ToList();

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var createResult = await roleManager.CreateAsync(new AppRole(roleName));
                if (!createResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Failed to create role '{roleName}': {DescribeErrors(createResult)}");
            }

            var role = await roleManager.FindByNameAsync(roleName)
                ?? throw new InvalidOperationException($"Role '{roleName}' could not be found after creation.");

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPerms = existingClaims
                .Where(c => c.Type == "perm")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Add missing permissions
            foreach (var perm in permissions.Where(p => !existingPerms.Contains(p)))
            {
                var addResult = await roleManager.AddClaimAsync(role, new Claim("perm", perm));
                if (!addResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Failed to add permission '{perm}' to role '{roleName}': {DescribeErrors(addResult)}");
            }

            // Remove stale permissions
            var targetPerms = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var claim in existingClaims.Where(c => c.Type == "perm" && !targetPerms.Contains(c.Value)))
            {
                var removeResult = await roleManager.RemoveClaimAsync(role, claim);
                if (!removeResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Failed to remove stale permission '{claim.Value}' from role '{roleName}': {DescribeErrors(removeResult)}");
            }
        }
    }

    private static string DescribeErrors(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(e => e.Description));

    private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager)
    {
        const string adminUsername = "admin@local.dev";
        const string adminPassword = "Password123!";

        var admins = await userManager.GetUsersInRoleAsync(Roles.Admin);

        if (admins.Count != 0)
            return;

        var adminUser = await userManager.FindByNameAsync(adminUsername);

        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                UserName = adminUsername,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }
        }

        await userManager.AddToRoleAsync(adminUser, Roles.Admin);
    }

    private static async Task BackfillUserPersonnelLinksAsync(UserManager<AppUser> userManager, AppDbContext db)
    {
        var usersWithoutPersonnel = await userManager.Users
            .Where(u => u.IsActive && u.PersonnelId == null)
            .ToListAsync();

        if (usersWithoutPersonnel.Count == 0)
            return;

        foreach (var user in usersWithoutPersonnel)
        {
            var normalizedEmail = NormalizeEmail(user.UserName, user.Id);

            var personnel = await db.Personnel
                .FirstOrDefaultAsync(p => p.Email == normalizedEmail);

            if (personnel is null)
            {
                var localPart = normalizedEmail.Split('@')[0];
                var nameParts = localPart
                    .Split(new[] { '.', '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var roleNames = await userManager.GetRolesAsync(user);
                personnel = new Personnel
                {
                    FirstName = nameParts.Length > 0 ? ToTitle(nameParts[0]) : "User",
                    LastName = nameParts.Length > 1 ? ToTitle(nameParts[1]) : "Account",
                    Email = normalizedEmail,
                    Function = MapRoleToPersonnelFunction(roleNames.FirstOrDefault()),
                };

                db.Personnel.Add(personnel);
                await db.SaveChangesAsync();
            }

            user.PersonnelId = personnel.Id;
            await userManager.UpdateAsync(user);
        }
    }

    private static PersonnelFunction MapRoleToPersonnelFunction(string? roleName)
    => roleName switch
    {
        Roles.Professor => PersonnelFunction.Professor,
        Roles.LabInstructor => PersonnelFunction.LabInstructor,
        Roles.CourseInstructor => PersonnelFunction.CourseInstructor,
        _ => PersonnelFunction.Professor,
    };

    private static string NormalizeEmail(string? username, int userId)
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
