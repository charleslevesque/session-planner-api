using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Tests.Integration.Fixtures;

/// <summary>
/// Auth handler that simulates an admin-level user with ID=1 linked to a seeded Personnel{Id=1}.
/// Has all permissions including TeachingNeeds. NOT in Teacher role → uses the admin code path.
/// </summary>
public class TeachingNeedTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestTeachingNeed";

    public TeachingNeedTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "TestAdmin"),

            // Sessions
            new("perm", Permissions.Sessions.Read),
            new("perm", Permissions.Sessions.Create),
            new("perm", Permissions.Sessions.Update),
            new("perm", Permissions.Sessions.Delete),

            // Courses
            new("perm", Permissions.Courses.Read),
            new("perm", Permissions.Courses.Create),
            new("perm", Permissions.Courses.Update),
            new("perm", Permissions.Courses.Delete),

            // Personnels
            new("perm", Permissions.Personnels.Read),
            new("perm", Permissions.Personnels.Create),
            new("perm", Permissions.Personnels.Update),
            new("perm", Permissions.Personnels.Delete),

            // TeachingNeeds — all permissions
            new("perm", Permissions.TeachingNeeds.Read),
            new("perm", Permissions.TeachingNeeds.Create),
            new("perm", Permissions.TeachingNeeds.Update),
            new("perm", Permissions.TeachingNeeds.Delete),

            // Softwares / OS (for items)
            new("perm", Permissions.Softwares.Read),
            new("perm", Permissions.Softwares.Create),
            new("perm", Permissions.OperatingSystems.Read),
            new("perm", Permissions.OperatingSystems.Create),
            new("perm", Permissions.SoftwareVersions.Read),
            new("perm", Permissions.SoftwareVersions.Create),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Factory that seeds a Personnel{Id=1} and User{Id=1, PersonnelId=1} so that
/// the TeachingNeedTestAuthHandler (NameIdentifier="1") passes ownership checks.
/// </summary>
public class TeachingNeedWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    /// <summary>Personnel ID used by the seeded test user — use as PersonnelId in Create requests.</summary>
    public const int SeededPersonnelId = 1;

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
                services.Remove(descriptor);

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TeachingNeedTestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TeachingNeedTestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TeachingNeedTestAuthHandler>(
                TeachingNeedTestAuthHandler.SchemeName,
                _ => { });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            // Seed the personnel and user so IsOwner() resolves correctly.
            db.Personnel.Add(new Personnel
            {
                Id = SeededPersonnelId,
                FirstName = "Jean",
                LastName = "Dupont",
                Function = PersonnelFunction.Professor,
                Email = "jean.dupont@test.com"
            });
            db.Users.Add(new User
            {
                Id = 1,
                Username = "testadmin",
                PasswordHash = "not-used-in-tests",
                IsActive = true,
                PersonnelId = SeededPersonnelId
            });
            db.SaveChanges();
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _connection?.Dispose();

        base.Dispose(disposing);
    }
}
