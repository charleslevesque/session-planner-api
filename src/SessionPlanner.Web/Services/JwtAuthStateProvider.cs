using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace SessionPlanner.Web.Services;

public sealed class JwtAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly AuthSessionService _auth;
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public JwtAuthStateProvider(AuthSessionService auth)
    {
        _auth = auth;
        _auth.StateChanged += OnAuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await _auth.InitializeAsync();

        if (string.IsNullOrWhiteSpace(_auth.AccessToken))
        {
            return new AuthenticationState(Anonymous);
        }

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(_auth.AccessToken), "jwt", ClaimTypes.Name, ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private void OnAuthStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void Dispose() =>
        _auth.StateChanged -= OnAuthStateChanged;

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return [];
        }

        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
        var claims = new List<Claim>();
        using var document = JsonDocument.Parse(payloadJson);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in property.Value.EnumerateArray())
                {
                    claims.Add(new Claim(property.Name, element.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(property.Name, property.Value.ToString()));
            }
        }

        return claims;
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2:
                padded += "==";
                break;
            case 3:
                padded += "=";
                break;
        }

        return Convert.FromBase64String(padded);
    }
}
