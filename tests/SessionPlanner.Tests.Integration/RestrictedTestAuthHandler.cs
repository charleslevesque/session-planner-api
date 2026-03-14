using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SessionPlanner.Core.Auth;

namespace SessionPlanner.Tests.Integration;

/// <summary>
/// Auth handler that simulates a Teacher user with read-only access (no session update/create/delete).
/// </summary>
public class RestrictedTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestRestricted";

    public RestrictedTestAuthHandler(
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
            new Claim(ClaimTypes.Name, "TeacherUser"),
            new Claim(ClaimTypes.Role, Roles.Teacher),

            new Claim("perm", Permissions.Sessions.Read),
            new Claim("perm", Permissions.Courses.Read),
            new Claim("perm", Permissions.TeachingNeeds.Read),
            new Claim("perm", Permissions.TeachingNeeds.Create),
            new Claim("perm", Permissions.TeachingNeeds.Update),
            new Claim("perm", Permissions.TeachingNeeds.Delete),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
