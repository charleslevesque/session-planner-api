using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

/// <summary>
/// Verifies that course resource endpoints are accessible with courses:read permission
/// (RestrictedWebApplicationFactory has courses:read) and that CRUD write operations are blocked.
/// </summary>
public class CourseResourcesAuthorizationTests : IClassFixture<RestrictedWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/Courses";

    public CourseResourcesAuthorizationTests(RestrictedWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetResources_WithReadPermission_DoesNotReturn403()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/resources");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSaaS_WithReadPermission_DoesNotReturn403()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/saas");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSoftwares_WithReadPermission_DoesNotReturn403()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/softwares");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetConfigurations_WithReadPermission_DoesNotReturn403()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/configurations");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetVms_WithReadPermission_DoesNotReturn403()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/vms");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetServers_WithReadPermission_DoesNotReturn403()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/servers");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetEquipment_WithReadPermission_DoesNotReturn403()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/equipment");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCourse_WithoutCreatePermission_ReturnsForbidden()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { Code = "TEST", Name = "Test" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCourse_WithoutUpdatePermission_ReturnsForbidden()
    {
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/1", new { Code = "TEST", Name = "Test" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCourse_WithoutDeletePermission_ReturnsForbidden()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
