using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Enums;

namespace SessionPlanner.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<
    AppUser,
    AppRole,
    int,
    IdentityUserClaim<int>,
    AppUserRole,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Main entities
    public DbSet<Software> Softwares => Set<Software>();
    public DbSet<SoftwareVersion> SoftwareVersions => Set<SoftwareVersion>();
    public DbSet<Laboratory> Laboratories => Set<Laboratory>();
    public DbSet<OS> OperatingSystems => Set<OS>();
    public DbSet<Workstation> Workstations => Set<Workstation>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public DbSet<VirtualMachine> VirtualMachines => Set<VirtualMachine>();
    public DbSet<PhysicalServer> PhysicalServers => Set<PhysicalServer>();
    public DbSet<SaaSProduct> SaaSProducts => Set<SaaSProduct>();
    public DbSet<EquipmentModel> EquipmentModels => Set<EquipmentModel>();
    public DbSet<Personnel> Personnel => Set<Personnel>();
    public DbSet<TeachingNeed> TeachingNeeds => Set<TeachingNeed>();
    public DbSet<TeachingNeedItem> TeachingNeedItems => Set<TeachingNeedItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LaboratorySoftware> LaboratorySoftwares => Set<LaboratorySoftware>();
    public DbSet<SessionCourse> SessionCourses => Set<SessionCourse>();

    // Join entity DbSets — all join tables are registered here for consistency
    public DbSet<CourseSoftware> CourseSoftwares => Set<CourseSoftware>();
    public DbSet<CourseSoftwareVersion> CourseSoftwareVersions => Set<CourseSoftwareVersion>();
    public DbSet<CourseLaboratory> CourseLaboratories => Set<CourseLaboratory>();
    public DbSet<CourseConfiguration> CourseConfigurations => Set<CourseConfiguration>();
    public DbSet<CourseVirtualMachine> CourseVirtualMachines => Set<CourseVirtualMachine>();
    public DbSet<CoursePhysicalServer> CoursePhysicalServers => Set<CoursePhysicalServer>();
    public DbSet<CourseSaaSProduct> CourseSaaSProducts => Set<CourseSaaSProduct>();
    public DbSet<CourseEquipmentModel> CourseEquipmentModels => Set<CourseEquipmentModel>();
    public DbSet<CoursePersonnel> CoursePersonnels => Set<CoursePersonnel>();
    public DbSet<LaboratoryConfiguration> LaboratoryConfigurations => Set<LaboratoryConfiguration>();
    public DbSet<ConfigurationOS> ConfigurationOSes => Set<ConfigurationOS>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity must be configured first
        base.OnModelCreating(modelBuilder);

        // Extend the Identity User → UserRole navigation with our AppUserRole type
        modelBuilder.Entity<AppUser>(b =>
        {
            b.HasMany<AppUserRole>(u => u.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Add Role navigation on AppUserRole
        modelBuilder.Entity<AppUserRole>(b =>
        {
            b.HasOne<AppRole>(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Personnel)
            .WithOne(p => p.User)
            .HasForeignKey<AppUser>(u => u.PersonnelId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Session>()
            .HasOne(s => s.CreatedByUser)
            .WithMany()
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure composite keys for join entities
        modelBuilder.Entity<CourseSoftware>()
            .HasKey(cs => new { cs.CourseId, cs.SoftwareId });

        modelBuilder.Entity<CourseLaboratory>()
            .HasKey(cl => new { cl.CourseId, cl.LaboratoryId });

        modelBuilder.Entity<CourseConfiguration>()
            .HasKey(cc => new { cc.CourseId, cc.ConfigurationId });

        modelBuilder.Entity<CourseVirtualMachine>()
            .HasKey(cvm => new { cvm.CourseId, cvm.VirtualMachineId });

        modelBuilder.Entity<CoursePhysicalServer>()
            .HasKey(cps => new { cps.CourseId, cps.PhysicalServerId });

        modelBuilder.Entity<CourseSaaSProduct>()
            .HasKey(csp => new { csp.CourseId, csp.SaaSProductId });

        modelBuilder.Entity<CourseEquipmentModel>()
            .HasKey(cem => new { cem.CourseId, cem.EquipmentModelId });

        modelBuilder.Entity<CourseSoftwareVersion>()
            .HasKey(csv => new { csv.CourseId, csv.SoftwareVersionId });

        modelBuilder.Entity<CoursePersonnel>()
            .HasKey(cp => new { cp.CourseId, cp.PersonnelId });

        modelBuilder.Entity<LaboratoryConfiguration>()
            .HasKey(lc => new { lc.LaboratoryId, lc.ConfigurationId });

        modelBuilder.Entity<LaboratorySoftware>()
            .HasKey(ls => new { ls.LaboratoryId, ls.SoftwareId });

        modelBuilder.Entity<ConfigurationOS>()
            .HasKey(co => new { co.ConfigurationId, co.OSId });

        modelBuilder.Entity<SessionCourse>()
            .HasKey(sc => new { sc.SessionId, sc.CourseId });

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Store NeedItemType enum as its snake_case string representation (e.g. "virtual_machine")
        // to match the values already persisted in the database and expected by the frontend.
        modelBuilder.Entity<TeachingNeedItem>()
            .Property(i => i.ItemType)
            .HasMaxLength(30)
            .HasConversion(
                v => JsonNamingPolicy.SnakeCaseLower.ConvertName(v.ToString()),
                v => StringToNeedItemType(v));
    }

    private static NeedItemType StringToNeedItemType(string value)
    {
        var normalized = value.Replace("_", "");

        if (Enum.TryParse<NeedItemType>(normalized, ignoreCase: true, out var result) && Enum.IsDefined(result))
            return result;

        // Corrupt/unrecognised DB values fall back to Other so the row can still be loaded.
        // This is logged implicitly — callers should investigate any unexpected Other values.
        return NeedItemType.Other;
    }
}