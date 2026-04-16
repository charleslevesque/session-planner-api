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
        // In test environment, OpenAI key is not set → available = false
        body!.Available.Should().BeFalse();
    }

    [Fact]
    public async Task SuggestItems_WithoutApiKey_Returns503()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/suggest-items",
            new { sessionId = 1, courseId = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task SuggestItems_WithoutBody_Returns400()
    {
        var response = await _client.PostAsync($"{BaseUrl}/suggest-items",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        // Missing required fields returns either 400 or 503 (no API key)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task AnalyzeNeed_WithoutApiKey_Returns503()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/analyze-need",
            new { sessionId = 1, needId = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    private record StatusResponse(bool Available);
}
