using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class AiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/Ai";

    public AiControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Status_ReturnsAvailableField()
    {
        var response = await _client.GetAsync($"{BaseUrl}/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StatusResponse>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task SuggestItems_ReturnsSuccessOrServiceUnavailable()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/suggest-items",
            new { sessionId = 1, courseId = 1 });

        // 503 when API key not configured, 200 when configured (even if OpenAI call fails, we return 200 with error message)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    private record StatusResponse(bool Available);
}
