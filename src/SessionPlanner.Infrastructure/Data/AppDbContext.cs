using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<CoursePersonnel>()
            .HasKey(cp => new { cp.CourseId, cp.PersonnelId });

        modelBuilder.Entity<LaboratoryConfiguration>()
            .HasKey(lc => new { lc.LaboratoryId, lc.ConfigurationId });

        modelBuilder.Entity<WorkstationSoftware>()
            .HasKey(ws => new { ws.WorkstationId, ws.SoftwareId });

        modelBuilder.Entity<VirtualMachineSoftware>()
            .HasKey(vms => new { vms.VirtualMachineId, vms.SoftwareId });

        modelBuilder.Entity<VirtualMachineConfiguration>()
            .HasKey(vmc => new { vmc.VirtualMachineId, vmc.ConfigurationId });

        modelBuilder.Entity<PhysicalServerSoftware>()
            .HasKey(pss => new { pss.PhysicalServerId, pss.SoftwareId });

        modelBuilder.Entity<PhysicalServerConfiguration>()
            .HasKey(psc => new { psc.PhysicalServerId, psc.ConfigurationId });

        modelBuilder.Entity<SoftwareOS>()
            .HasKey(so => new { so.SoftwareId, so.OSId });

        modelBuilder.Entity<ConfigurationOS>()
            .HasKey(co => new { co.ConfigurationId, co.OSId });
    }
}