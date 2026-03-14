using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SessionPlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;

namespace SessionPlanner.Tests.Integration.Fixtures;

/// <summary>
/// Factory that authenticates with a Teacher role (no session permissions) to test 403 scenarios.
/// </summary>
public class RestrictedWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<AppDbContext>();

            var dbContextDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("DbContext") == true)
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = RestrictedTestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = RestrictedTestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, RestrictedTestAuthHandler>(
                RestrictedTestAuthHandler.SchemeName,
                _ => { });

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        base.Dispose(disposing);
    }
}
