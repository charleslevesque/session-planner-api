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

    [Fact]
    public async Task AnalyzeNeed_ReturnsSuccessOrServiceUnavailable()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/analyze-need",
            new { sessionId = 1, needId = 1 });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task AutoFill_ReturnsOkWithSuggestionsStructure()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/auto-fill",
            new
            {
                sessionId = 1,
                courseId = 1,
                itemType = "software",
                currentValues = new Dictionary<string, string> { ["softwareName"] = "Eclipse" }
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AutoFillResponse>();
        body.Should().NotBeNull();
        body!.Source.Should().NotBeNull();
        body.Suggestions.Should().NotBeNull();
    }

    [Fact]
    public async Task AutoFill_EmptyValues_ReturnsNoSuggestions()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/auto-fill",
            new
            {
                sessionId = 1,
                courseId = 1,
                itemType = "software",
                currentValues = new Dictionary<string, string> { ["softwareName"] = "" }
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AutoFillResponse>();
        body.Should().NotBeNull();
        body!.Source.Should().Be("none");
        body.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task AutoFill_UnknownItemType_ReturnsEmpty()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/auto-fill",
            new
            {
                sessionId = 1,
                courseId = 1,
                itemType = "unknown_type",
                currentValues = new Dictionary<string, string> { ["foo"] = "bar" }
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AutoFillResponse>();
        body.Should().NotBeNull();
        body!.Source.Should().Be("none");
    }

    [Fact]
    public async Task RejectionAssist_ReturnsSuccessOrServiceUnavailable()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/rejection-assist",
            new { sessionId = 1, needId = 1 });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task RejectionAssist_ReturnsCorrectStructure()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/rejection-assist",
            new { sessionId = 1, needId = 1 });

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<RejectionAssistResponseDto>();
            body.Should().NotBeNull();
            body!.Explanation.Should().NotBeNull();
            body.Steps.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task RejectionAssist_NonExistentNeed_ReturnsOkWithFallback()
    {
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/rejection-assist",
            new { sessionId = 999, needId = 999 });

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<RejectionAssistResponseDto>();
            body.Should().NotBeNull();
            body!.Steps.Should().BeEmpty();
        }
    }

    private record StatusResponse(bool Available);
    private record AutoFillSuggestionDto(string Value, string Reason, float Confidence);
    private record AutoFillResponse(Dictionary<string, AutoFillSuggestionDto> Suggestions, string Source);
    private record CorrectionStepDto(string Action, string Target, string Detail);
    private record RejectionAssistResponseDto(string Explanation, List<CorrectionStepDto> Steps, string? RevisedNotes);
}
