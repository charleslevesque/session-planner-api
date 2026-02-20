using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Software> Softwares => Set<Software>();
    public DbSet<SoftwareVersion> SoftwareVersions => Set<SoftwareVersion>();
    public DbSet<Laboratory> Laboratories => Set<Laboratory>();
    public DbSet<OS> OperatingSystems => Set<OS>();
     public DbSet<Session> Sessions => Set<Session>();
}