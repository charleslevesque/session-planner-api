using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Auth;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class AuthControllerTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/Auth";
    private const string AdminEmail = "admin@local.dev";
    private const string AdminPassword = "Password123!";

    public AuthControllerTests(AuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<AuthResponse> LoginAsAdminAsync()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/login",
            new LoginRequest(AdminEmail, AdminPassword));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/login",
            new LoginRequest(AdminEmail, AdminPassword));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/login",
            new LoginRequest(AdminEmail, "WrongPassword1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/login",
            new LoginRequest("noone@test.com", "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Me Tests

    [Fact]
    public async Task Me_WithValidToken_ReturnsProfile()
    {
        var tokens = await LoginAsAdminAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Token);
        var response = await _client.GetAsync($"{BaseUrl}/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me!.Email.Should().Be(AdminEmail);
        me.Role.Should().Be("admin");
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var client = _client;
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.GetAsync($"{BaseUrl}/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Refresh Tests

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var tokens = await LoginAsAdminAsync();

        var response = await _client.PostAsJsonAsync($"{BaseUrl}/refresh",
            new { refreshToken = tokens.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await response.Content.ReadFromJsonAsync<AuthResponse>();
        newTokens!.Token.Should().NotBeNullOrEmpty();
        newTokens.RefreshToken.Should().NotBe(tokens.RefreshToken);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_ReturnsNoContent()
    {
        var tokens = await LoginAsAdminAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Token);
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/logout",
            new { refreshToken = tokens.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion
}

