using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Sessions;
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
    public async Task GetAll_WhenEmpty_ReturnsOk()
    {
        var response = await _client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithActiveFilter_ReturnsOk()
    {
        var response = await _client.GetAsync($"{BaseUrl}?active=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var request = new CreateSessionRequest("Hiver 2026", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        session.Should().NotBeNull();
        session!.Title.Should().Be("Hiver 2026");
        session.Status.Should().Be(Core.Enums.SessionStatus.Draft);
    }

    [Fact]
    public async Task Create_WithInvalidDates_ReturnsBadRequest()
    {
        var request = new CreateSessionRequest("Bad", DateTime.UtcNow.AddMonths(4), DateTime.UtcNow);

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task Update_ExistingSession_ReturnsOk()
    {
        var create = new CreateSessionRequest("Original", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var update = new UpdateSessionRequest("Updated", DateTime.UtcNow, DateTime.UtcNow.AddMonths(5));
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{created!.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<SessionResponse>();
        updated!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_NonExistent_Returns404()
    {
        var update = new UpdateSessionRequest("X", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/9999", update);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_DraftSession_ReturnsNoContent()
    {
        var create = new CreateSessionRequest("ToDelete", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var response = await _client.DeleteAsync($"{BaseUrl}/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region Transition Tests

    [Fact]
    public async Task Open_DraftSession_ReturnsOpenSessionWithTimestamp()
    {
        var create = new CreateSessionRequest("ToOpen", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var response = await _client.PostAsync($"{BaseUrl}/{created!.Id}/open", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        session!.Status.Should().Be(Core.Enums.SessionStatus.Open);
        session.OpenedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Close_OpenSession_ReturnsClosedSession()
    {
        var create = new CreateSessionRequest("ToClose", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        await _client.PostAsync($"{BaseUrl}/{created!.Id}/open", null);
        var response = await _client.PostAsync($"{BaseUrl}/{created.Id}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        session!.Status.Should().Be(Core.Enums.SessionStatus.Closed);
        session.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Archive_ClosedSession_ReturnsArchivedSession()
    {
        var create = new CreateSessionRequest("ToArchive", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        await _client.PostAsync($"{BaseUrl}/{created!.Id}/open", null);
        await _client.PostAsync($"{BaseUrl}/{created.Id}/close", null);
        var response = await _client.PostAsync($"{BaseUrl}/{created.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        session!.Status.Should().Be(Core.Enums.SessionStatus.Archived);
        session.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Open_AlreadyOpenSession_ReturnsBadRequest()
    {
        var create = new CreateSessionRequest("AlreadyOpen", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        await _client.PostAsync($"{BaseUrl}/{created!.Id}/open", null);
        var response = await _client.PostAsync($"{BaseUrl}/{created.Id}/open", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Close_DraftSession_ReturnsBadRequest()
    {
        var create = new CreateSessionRequest("DraftClose", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var response = await _client.PostAsync($"{BaseUrl}/{created!.Id}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Archive_DraftSession_ReturnsBadRequest()
    {
        var create = new CreateSessionRequest("DraftArchive", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var response = await _client.PostAsync($"{BaseUrl}/{created!.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Archive_OpenSession_ReturnsBadRequest()
    {
        var create = new CreateSessionRequest("OpenArchive", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        await _client.PostAsync($"{BaseUrl}/{created!.Id}/open", null);
        var response = await _client.PostAsync($"{BaseUrl}/{created.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
