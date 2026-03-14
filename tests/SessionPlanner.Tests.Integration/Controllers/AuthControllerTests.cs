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

    public AuthControllerTests(AuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        var request = new RegisterRequest("newuser@test.com", "Password123", "Jean", "Tremblay");

        var response = await _client.PostAsJsonAsync($"{BaseUrl}/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var request = new RegisterRequest("duplicate@test.com", "Password123", "A", "B");
        await _client.PostAsJsonAsync($"{BaseUrl}/register", request);

        var response = await _client.PostAsJsonAsync($"{BaseUrl}/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_InvalidEmail_ReturnsBadRequest()
    {
        var request = new RegisterRequest("not-an-email", "Password123", "A", "B");

        var response = await _client.PostAsJsonAsync($"{BaseUrl}/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShortPassword_ReturnsBadRequest()
    {
        var request = new RegisterRequest("short@test.com", "abc", "A", "B");

        var response = await _client.PostAsJsonAsync($"{BaseUrl}/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        var email = "login@test.com";
        await _client.PostAsJsonAsync($"{BaseUrl}/register",
            new RegisterRequest(email, "Password123", "A", "B"));

        var response = await _client.PostAsJsonAsync($"{BaseUrl}/login",
            new LoginRequest(email, "Password123"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = "wrongpw@test.com";
        await _client.PostAsJsonAsync($"{BaseUrl}/register",
            new RegisterRequest(email, "Password123", "A", "B"));

        var response = await _client.PostAsJsonAsync($"{BaseUrl}/login",
            new LoginRequest(email, "WrongPassword"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/login",
            new LoginRequest("noone@test.com", "Password123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Me Tests

    [Fact]
    public async Task Me_WithValidToken_ReturnsProfile()
    {
        var email = "me@test.com";
        await _client.PostAsJsonAsync($"{BaseUrl}/register",
            new RegisterRequest(email, "Password123", "Marie", "Dupont"));

        var loginResponse = await _client.PostAsJsonAsync($"{BaseUrl}/login",
            new LoginRequest(email, "Password123"));
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.Token);
        var response = await _client.GetAsync($"{BaseUrl}/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me!.Email.Should().Be(email);
        me.Name.Should().Be("Marie Dupont");
        me.Role.Should().Be("teacher");
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
        var email = "refresh@test.com";
        var registerResponse = await _client.PostAsJsonAsync($"{BaseUrl}/register",
            new RegisterRequest(email, "Password123", "A", "B"));
        var tokens = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var response = await _client.PostAsJsonAsync($"{BaseUrl}/refresh",
            new { refreshToken = tokens!.RefreshToken });

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
        var email = "logout@test.com";
        var registerResponse = await _client.PostAsJsonAsync($"{BaseUrl}/register",
            new RegisterRequest(email, "Password123", "A", "B"));
        var tokens = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.Token);
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/logout",
            new { refreshToken = tokens.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion
}
