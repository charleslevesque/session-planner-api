using Microsoft.AspNetCore.Components.Authorization;
using SessionPlanner.Web.Models;
using System.Security.Claims;

namespace SessionPlanner.Web.Services;

public class AppAuthStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _auth;

    public AppAuthStateProvider(IAuthService auth)
    {
        _auth = auth;
        _auth.OnAuthStateChanged += () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await _auth.GetStoredUserAsync();
        if (user == null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
        };
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyAuthStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
