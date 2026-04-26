using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Auth;
using SessionPlanner.Api.Dtos.Users;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class UsersControllerTests
{
    private const string AuthBaseUrl = "/api/v1/Auth";
    private const string UsersBaseUrl = "/api/v1/Users";

    private async Task<(HttpClient client, string token)> LoginAsAdminAsync(AuthWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            $"{AuthBaseUrl}/login",
            new LoginRequest("admin@local.dev", "Password123!"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.Token);
        return (client, tokens.Token);
    }

    private async Task<int> CreateTeacherAndGetIdAsync(HttpClient client, string? email = null)
    {
        email ??= $"teacher.{Guid.NewGuid():N}@local.dev";
        await client.PostAsJsonAsync(
            $"{UsersBaseUrl}",
            new { username = email, password = "Password123!", roleName = "professor" });

        var usersResponse = await client.GetFromJsonAsync<UserResponse[]>($"{UsersBaseUrl}?includeInactive=true");
        return usersResponse!.First(u => u.Username == email).Id;
    }

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

        // Create a teacher account via admin endpoint
        var (adminClient, _) = await LoginAsAdminAsync(factory);
        var teacherEmail = $"teacher.{Guid.NewGuid():N}@local.dev";
        await adminClient.PostAsJsonAsync(
            $"{UsersBaseUrl}",
            new { username = teacherEmail, password = "Password123!", roleName = "professor" });

        var loginResponse = await client.PostAsJsonAsync(
            $"{AuthBaseUrl}/login",
            new LoginRequest(teacherEmail, "Password123!"));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        tokens.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.Token);

        var updateResponse = await client.PutAsJsonAsync(
            $"{UsersBaseUrl}/me/email",
            new { newEmail = $"updated.{Guid.NewGuid():N}@local.dev", currentPassword = "Password123!" });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCurrentUserEmail_WithDuplicateEmail_ReturnsBadRequest()
    {
        await using var factory = new AuthWebApplicationFactory();
        using var client = factory.CreateClient();

        // Create an existing user via the admin endpoint so its email is taken
        var (adminClient, _) = await LoginAsAdminAsync(factory);
        var existingEmail = $"existing.{Guid.NewGuid():N}@local.dev";
        await adminClient.PostAsJsonAsync(
            $"{UsersBaseUrl}",
            new { username = existingEmail, password = "Password123!", roleName = "professor" });

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

    [Fact]
    public async Task Deactivate_Teacher_ReturnsNoContent_And_UserBecomesInactive()
    {
        await using var factory = new AuthWebApplicationFactory();
        var (client, _) = await LoginAsAdminAsync(factory);

        var teacherId = await CreateTeacherAndGetIdAsync(client);

        var deactivateResponse = await client.PostAsync($"{UsersBaseUrl}/{teacherId}/deactivate", null);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var usersResponse = await client.GetFromJsonAsync<UserResponse[]>($"{UsersBaseUrl}?includeInactive=true");
        var teacher = usersResponse!.FirstOrDefault(u => u.Id == teacherId);
        teacher.Should().NotBeNull();
        teacher!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_Admin_ReturnsBadRequest()
    {
        await using var factory = new AuthWebApplicationFactory();
        var (client, _) = await LoginAsAdminAsync(factory);

        var usersResponse = await client.GetFromJsonAsync<UserResponse[]>($"{UsersBaseUrl}");
        var admin = usersResponse!.First(u => u.Roles == "admin");

        var response = await client.PostAsync($"{UsersBaseUrl}/{admin.Id}/deactivate", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deactivate_NonExistentUser_ReturnsNotFound()
    {
        await using var factory = new AuthWebApplicationFactory();
        var (client, _) = await LoginAsAdminAsync(factory);

        var response = await client.PostAsync($"{UsersBaseUrl}/99999/deactivate", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reactivate_DeactivatedUser_ReturnsNoContent_And_UserBecomesActive()
    {
        await using var factory = new AuthWebApplicationFactory();
        var (client, _) = await LoginAsAdminAsync(factory);

        var teacherId = await CreateTeacherAndGetIdAsync(client);

        await client.PostAsync($"{UsersBaseUrl}/{teacherId}/deactivate", null);

        var reactivateResponse = await client.PostAsync($"{UsersBaseUrl}/{teacherId}/reactivate", null);
        reactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var usersResponse = await client.GetFromJsonAsync<UserResponse[]>($"{UsersBaseUrl}?includeInactive=true");
        var teacher = usersResponse!.FirstOrDefault(u => u.Id == teacherId);
        teacher.Should().NotBeNull();
        teacher!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Reactivate_AlreadyActiveUser_ReturnsBadRequest()
    {
        await using var factory = new AuthWebApplicationFactory();
        var (client, _) = await LoginAsAdminAsync(factory);

        var teacherId = await CreateTeacherAndGetIdAsync(client);

        var response = await client.PostAsync($"{UsersBaseUrl}/{teacherId}/reactivate", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeactivatedUser_CannotLogin()
    {
        await using var factory = new AuthWebApplicationFactory();
        var email = $"teacher.{Guid.NewGuid():N}@local.dev";

        var (adminClient, _) = await LoginAsAdminAsync(factory);
        var teacherId = await CreateTeacherAndGetIdAsync(adminClient, email);

        await adminClient.PostAsync($"{UsersBaseUrl}/{teacherId}/deactivate", null);

        using var anonClient = factory.CreateClient();
        var loginResponse = await anonClient.PostAsJsonAsync(
            $"{AuthBaseUrl}/login",
            new LoginRequest(email, "Password123!"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivatedUser_DataPreserved_AdminCanStillSeeUser()
    {
        await using var factory = new AuthWebApplicationFactory();
        var email = $"teacher.{Guid.NewGuid():N}@local.dev";

        var (client, _) = await LoginAsAdminAsync(factory);
        var teacherId = await CreateTeacherAndGetIdAsync(client, email);

        await client.PostAsync($"{UsersBaseUrl}/{teacherId}/deactivate", null);

        var userResponse = await client.GetAsync($"{UsersBaseUrl}/{teacherId}");
        userResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();
        user.Should().NotBeNull();
        user!.Username.Should().Be(email);
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_WithIncludeInactive_ReturnsDeactivatedUsers()
    {
        await using var factory = new AuthWebApplicationFactory();
        var (client, _) = await LoginAsAdminAsync(factory);

        var teacherId = await CreateTeacherAndGetIdAsync(client);
        await client.PostAsync($"{UsersBaseUrl}/{teacherId}/deactivate", null);

        var allUsers = await client.GetFromJsonAsync<UserResponse[]>($"{UsersBaseUrl}?includeInactive=true");
        allUsers!.Should().Contain(u => u.Id == teacherId && !u.IsActive);

        var activeOnly = await client.GetFromJsonAsync<UserResponse[]>($"{UsersBaseUrl}");
        activeOnly!.Should().NotContain(u => u.Id == teacherId);
    }

    [Fact]
    public async Task ReactivatedUser_CanLoginAgain()
    {
        await using var factory = new AuthWebApplicationFactory();
        var email = $"teacher.{Guid.NewGuid():N}@local.dev";

        var (adminClient, _) = await LoginAsAdminAsync(factory);
        var teacherId = await CreateTeacherAndGetIdAsync(adminClient, email);

        await adminClient.PostAsync($"{UsersBaseUrl}/{teacherId}/deactivate", null);

        await adminClient.PostAsync($"{UsersBaseUrl}/{teacherId}/reactivate", null);

        using var anonClient = factory.CreateClient();
        var loginResponse = await anonClient.PostAsJsonAsync(
            $"{AuthBaseUrl}/login",
            new LoginRequest(email, "Password123!"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
