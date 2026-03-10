using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SessionPlanner.Core.Auth;

namespace SessionPlanner.Tests.Integration;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
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
            new Claim(ClaimTypes.Name, "SessionPlannerClient"),

            // Sessions
            new Claim("perm", Permissions.Sessions.Read),
            new Claim("perm", Permissions.Sessions.Create),
            new Claim("perm", Permissions.Sessions.Update),
            new Claim("perm", Permissions.Sessions.Delete),

            // Laboratories
            new Claim("perm", Permissions.Laboratories.Read),
            new Claim("perm", Permissions.Laboratories.Create),
            new Claim("perm", Permissions.Laboratories.Update),
            new Claim("perm", Permissions.Laboratories.Delete),

            // Operating Systems
            new Claim("perm", Permissions.OperatingSystems.Read),
            new Claim("perm", Permissions.OperatingSystems.Create),
            new Claim("perm", Permissions.OperatingSystems.Update),
            new Claim("perm", Permissions.OperatingSystems.Delete),

            // Softwares
            new Claim("perm", Permissions.Softwares.Read),
            new Claim("perm", Permissions.Softwares.Create),
            new Claim("perm", Permissions.Softwares.Update),
            new Claim("perm", Permissions.Softwares.Delete),

            // Software Versions
            new Claim("perm", Permissions.SoftwareVersions.Read),
            new Claim("perm", Permissions.SoftwareVersions.Create),
            new Claim("perm", Permissions.SoftwareVersions.Update),
            new Claim("perm", Permissions.SoftwareVersions.Delete),

            // Configurations
            new Claim("perm", Permissions.Configurations.Read),
            new Claim("perm", Permissions.Configurations.Create),
            new Claim("perm", Permissions.Configurations.Update),
            new Claim("perm", Permissions.Configurations.Delete),

            // Courses
            new Claim("perm", Permissions.Courses.Read),
            new Claim("perm", Permissions.Courses.Create),
            new Claim("perm", Permissions.Courses.Update),
            new Claim("perm", Permissions.Courses.Delete),

            // Equipment Models
            new Claim("perm", Permissions.EquipmentModels.Read),
            new Claim("perm", Permissions.EquipmentModels.Create),
            new Claim("perm", Permissions.EquipmentModels.Update),
            new Claim("perm", Permissions.EquipmentModels.Delete),

            // Personnels
            new Claim("perm", Permissions.Personnels.Read),
            new Claim("perm", Permissions.Personnels.Create),
            new Claim("perm", Permissions.Personnels.Update),
            new Claim("perm", Permissions.Personnels.Delete),

            // Physical Servers
            new Claim("perm", Permissions.PhysicalServers.Read),
            new Claim("perm", Permissions.PhysicalServers.Create),
            new Claim("perm", Permissions.PhysicalServers.Update),
            new Claim("perm", Permissions.PhysicalServers.Delete),

            // SaaS Products
            new Claim("perm", Permissions.SaaSProducts.Read),
            new Claim("perm", Permissions.SaaSProducts.Create),
            new Claim("perm", Permissions.SaaSProducts.Update),
            new Claim("perm", Permissions.SaaSProducts.Delete),

            // Virtual Machines
            new Claim("perm", Permissions.VirtualMachines.Read),
            new Claim("perm", Permissions.VirtualMachines.Create),
            new Claim("perm", Permissions.VirtualMachines.Update),
            new Claim("perm", Permissions.VirtualMachines.Delete),

            // Workstations
            new Claim("perm", Permissions.Workstations.Read),
            new Claim("perm", Permissions.Workstations.Create),
            new Claim("perm", Permissions.Workstations.Update),
            new Claim("perm", Permissions.Workstations.Delete),

            // Users
            new Claim("perm", Permissions.Users.Read),
            new Claim("perm", Permissions.Users.Create),
            new Claim("perm", Permissions.Users.Update),
            new Claim("perm", Permissions.Users.Delete),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}