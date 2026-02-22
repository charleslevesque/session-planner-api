using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Core.Entities;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class SessionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/Sessions";

    public SessionsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region GET Tests

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyList()
    {
        var response = await _client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessions = await response.Content.ReadFromJsonAsync<List<Session>>();
        sessions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_AfterCreate_ReturnsSessions()
    {
        var session = new Session
        {
            Title = "Test Session",
            Date = new DateTime(2026, 3, 15, 10, 0, 0)
        };
        await _client.PostAsJsonAsync(BaseUrl, session);

        var response = await _client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessions = await response.Content.ReadFromJsonAsync<List<Session>>();
        sessions.Should().NotBeNull();
        sessions.Should().Contain(s => s.Title == "Test Session");
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedSession()
    {
        var session = new Session
        {
            Title = "New Session",
            Date = new DateTime(2026, 4, 20, 14, 30, 0)
        };

        var response = await _client.PostAsJsonAsync(BaseUrl, session);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Session>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("New Session");
        created.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Morning Lab Session")]
    [InlineData("Afternoon Workshop")]
    [InlineData("Evening Review")]
    public async Task Create_WithVariousTitles_Succeeds(string title)
    {
        var session = new Session
        {
            Title = title,
            Date = DateTime.Now.AddDays(7)
        };

        var response = await _client.PostAsJsonAsync(BaseUrl, session);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Session>();
        created!.Title.Should().Be(title);
    }

    #endregion
}
