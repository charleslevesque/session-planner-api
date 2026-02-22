using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Tests.Integration.Fixtures;

/// <summary>
/// Custom WebApplicationFactory that replaces the SQLite database with an in-memory database for testing.
/// This is to make sure that the tests are isolated and don't affect the real database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "TestDb_" + Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all the DbContext-related registrations to avoid provider conflicts
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<AppDbContext>();
            
            // Also remove any IDbContextOptionsConfiguration
            var dbContextOptionsDescriptor = services
                .Where(d => d.ServiceType.FullName?.Contains("DbContext") == true)
                .ToList();
            
            foreach (var descriptor in dbContextOptionsDescriptor)
            {
                services.Remove(descriptor);
            }

            // Add an in-memory database for testing - use same name for this factory instance
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });

        builder.UseEnvironment("Testing");
    }
}
