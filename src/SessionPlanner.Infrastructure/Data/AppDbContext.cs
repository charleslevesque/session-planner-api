using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Enums;

namespace SessionPlanner.Infrastructure.Data;

public class AppDbContext : DbContext
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
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<TeachingNeed> TeachingNeeds => Set<TeachingNeed>();
    public DbSet<TeachingNeedItem> TeachingNeedItems => Set<TeachingNeedItem>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
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
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Personnel)
            .WithOne(p => p.User)
            .HasForeignKey<User>(u => u.PersonnelId)
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
        modelBuilder.Entity<UserPermission>()
            .HasKey(x => new { x.UserId, x.PermissionId });  
        
        modelBuilder.Entity<UserRole>()
            .HasKey(x => new { x.UserId, x.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId);

        modelBuilder.Entity<RolePermission>()
            .HasKey(x => new { x.RoleId, x.Permission });

        modelBuilder.Entity<RolePermission>()
            .HasOne(x => x.Role)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.RoleId);

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
        return Enum.TryParse<NeedItemType>(normalized, ignoreCase: true, out var result) ? result : NeedItemType.Software;
    }
}