using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Infrastructure.Auth;


public class JWTTokenService : IJWTTokenService
{
    private readonly IConfiguration _configuration;

    public JWTTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginTokenResponse CreateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var key = _configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key is missing.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is missing.");

        var expiryMinutesValue = _configuration["Jwt:ExpiryMinutes"]
            ?? throw new InvalidOperationException("Jwt:ExpiryMinutes is missing.");

        if (!int.TryParse(expiryMinutesValue, out var expiryMinutes))
            throw new InvalidOperationException("Jwt:ExpiryMinutes must be a valid integer.");

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username)
        };

        foreach (var role in roles.Distinct())
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions.Distinct())
        {
            claims.Add(new Claim("perm", permission));
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new LoginTokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt
        );
    } 
}