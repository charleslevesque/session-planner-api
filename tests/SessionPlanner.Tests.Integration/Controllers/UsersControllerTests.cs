using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Auth;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class UsersControllerTests
{
    private const string AuthBaseUrl = "/api/v1/Auth";
    private const string UsersBaseUrl = "/api/v1/Users";

    [Fact]
    public async Task UpdateCurrentUserEmail_AsAdmin_WithValidPayload_ReturnsNoContent()
    {
        await using var factory = new AuthWebApplicationFactory();
        using var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            $"{AuthBaseUrl}/login",
            new LoginRequest("admin@local.dev", "Password123!"));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        tokens.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.Token);

        var newEmail = $"admin.updated.{Guid.NewGuid():N}@local.dev";
        var updateResponse = await client.PutAsJsonAsync(
            $"{UsersBaseUrl}/me/email",
            new { newEmail, currentPassword = "Password123!" });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var meResponse = await client.GetAsync($"{AuthBaseUrl}/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await meResponse.Content.ReadFromJsonAsync<MeResponse>();
        me.Should().NotBeNull();
        me!.Email.Should().Be(newEmail);
    }

    [Fact]
    public async Task UpdateCurrentUserEmail_AsNonAdmin_ReturnsForbidden()
    {
        await using var factory = new AuthWebApplicationFactory();
        using var client = factory.CreateClient();

        var email = $"teacher.{Guid.NewGuid():N}@local.dev";
        await client.PostAsJsonAsync(
            $"{AuthBaseUrl}/register",
            new RegisterRequest(email, "Password123", "User", "Teacher"));

        var loginResponse = await client.PostAsJsonAsync(
            $"{AuthBaseUrl}/login",
            new LoginRequest(email, "Password123"));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        tokens.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.Token);

        var updateResponse = await client.PutAsJsonAsync(
            $"{UsersBaseUrl}/me/email",
            new { newEmail = $"updated.{Guid.NewGuid():N}@local.dev", currentPassword = "Password123" });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCurrentUserEmail_WithDuplicateEmail_ReturnsBadRequest()
    {
        await using var factory = new AuthWebApplicationFactory();
        using var client = factory.CreateClient();

        var existingEmail = $"existing.{Guid.NewGuid():N}@local.dev";
        await client.PostAsJsonAsync(
            $"{AuthBaseUrl}/register",
            new RegisterRequest(existingEmail, "Password123", "Existing", "User"));

        var loginResponse = await client.PostAsJsonAsync(
            $"{AuthBaseUrl}/login",
            new LoginRequest("admin@local.dev", "Password123!"));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        tokens.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.Token);

        var updateResponse = await client.PutAsJsonAsync(
            $"{UsersBaseUrl}/me/email",
            new { newEmail = existingEmail, currentPassword = "Password123!" });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCurrentUserEmail_WithoutToken_ReturnsUnauthorized()
    {
        await using var factory = new AuthWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"{UsersBaseUrl}/me/email",
            new { newEmail = "admin.new@local.dev", currentPassword = "Password123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
