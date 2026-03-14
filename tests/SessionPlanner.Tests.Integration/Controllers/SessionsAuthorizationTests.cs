using System.Net;
using FluentAssertions;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class SessionsAuthorizationTests : IClassFixture<RestrictedWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/Sessions";

    public SessionsAuthorizationTests(RestrictedWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Open_WithoutUpdatePermission_ReturnsForbidden()
    {
        var response = await _client.PostAsync($"{BaseUrl}/1/open", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Close_WithoutUpdatePermission_ReturnsForbidden()
    {
        var response = await _client.PostAsync($"{BaseUrl}/1/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Archive_WithoutUpdatePermission_ReturnsForbidden()
    {
        var response = await _client.PostAsync($"{BaseUrl}/1/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
