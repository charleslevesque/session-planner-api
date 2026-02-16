using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Session> Sessions => Set<Session>();
}