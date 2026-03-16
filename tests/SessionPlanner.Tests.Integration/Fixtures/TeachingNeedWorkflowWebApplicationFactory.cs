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

public class TeachingNeedWorkflowAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestTeachingNeedWorkflow";

    public TeachingNeedWorkflowAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var requestedRole = Request.Headers["x-test-role"].ToString().Trim().ToLowerInvariant();
        var isTeacher = requestedRole == Roles.Teacher;

        var userId = isTeacher ? 1 : 2;
        var userName = isTeacher ? "WorkflowTeacher" : "WorkflowAdmin";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Role, isTeacher ? Roles.Teacher : Roles.Admin),

            new("perm", Permissions.Sessions.Read),
            new("perm", Permissions.Sessions.Create),
            new("perm", Permissions.Sessions.Update),
            new("perm", Permissions.Sessions.Delete),

            new("perm", Permissions.Courses.Read),
            new("perm", Permissions.Courses.Create),
            new("perm", Permissions.Courses.Update),
            new("perm", Permissions.Courses.Delete),

            new("perm", Permissions.Personnels.Read),
            new("perm", Permissions.Personnels.Create),
            new("perm", Permissions.Personnels.Update),
            new("perm", Permissions.Personnels.Delete),

            new("perm", Permissions.TeachingNeeds.Read),
            new("perm", Permissions.TeachingNeeds.Create),
            new("perm", Permissions.TeachingNeeds.Update),
            new("perm", Permissions.TeachingNeeds.Delete)
        };

        if (!isTeacher)
        {
            claims.Add(new Claim(ClaimTypes.Role, Roles.Technician));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class TeachingNeedWorkflowWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    public const int TeacherPersonnelId = 1;
    public const int AdminPersonnelId = 2;

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
                options.DefaultAuthenticateScheme = TeachingNeedWorkflowAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TeachingNeedWorkflowAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TeachingNeedWorkflowAuthHandler>(
                TeachingNeedWorkflowAuthHandler.SchemeName,
                _ => { });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            db.Personnel.Add(new Personnel
            {
                Id = TeacherPersonnelId,
                FirstName = "Jean",
                LastName = "Dupont",
                Function = PersonnelFunction.Professor,
                Email = "workflow.teacher@test.com"
            });
            db.Personnel.Add(new Personnel
            {
                Id = AdminPersonnelId,
                FirstName = "Charles",
                LastName = "Tech",
                Function = PersonnelFunction.Professor,
                Email = "workflow.admin@test.com"
            });

            db.Users.Add(new User
            {
                Id = 1,
                Username = "workflow-teacher",
                PasswordHash = "not-used-in-tests",
                IsActive = true,
                PersonnelId = TeacherPersonnelId
            });
            db.Users.Add(new User
            {
                Id = 2,
                Username = "workflow-admin",
                PasswordHash = "not-used-in-tests",
                IsActive = true,
                PersonnelId = AdminPersonnelId
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
