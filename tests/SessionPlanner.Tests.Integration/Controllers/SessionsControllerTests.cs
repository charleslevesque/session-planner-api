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

    #region Helper

    private async Task<SessionResponse> CreateDraftSession(string title = "Test", List<int>? courseIds = null)
    {
        var request = new CreateSessionRequest(title, DateTime.UtcNow, DateTime.UtcNow.AddMonths(4), courseIds);
        var response = await _client.PostAsJsonAsync(BaseUrl, request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<SessionResponse>())!;
    }

    private async Task<int> CreateCourse(string code)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/Courses", new { code, name = code });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<SessionCourseResponse>();
        return body!.Id;
    }

    #endregion

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

    [Fact]
    public async Task Create_WithCourseIds_AssociatesCourses()
    {
        var cid1 = await CreateCourse("LOG100");
        var cid2 = await CreateCourse("LOG200");

        var request = new CreateSessionRequest("WithCourses", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4), new List<int> { cid1, cid2 });
        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        session!.CourseIds.Should().BeEquivalentTo(new[] { cid1, cid2 });
    }

    [Fact]
    public async Task Create_WithCopyFromSessionId_CopiesCourses()
    {
        var cid1 = await CreateCourse("LOG300");
        var cid2 = await CreateCourse("LOG400");

        var source = await CreateDraftSession("Source", new List<int> { cid1, cid2 });

        var request = new CreateSessionRequest("Copied", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4), CopyFromSessionId: source.Id);
        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        session!.CourseIds.Should().BeEquivalentTo(new[] { cid1, cid2 });
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

    #region Session Courses Tests

    [Fact]
    public async Task GetCourses_ReturnsAssociatedCourses()
    {
        var cid = await CreateCourse("LOG500");
        var session = await CreateDraftSession("HasCourse", new List<int> { cid });

        var response = await _client.GetAsync($"{BaseUrl}/{session.Id}/courses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var courses = await response.Content.ReadFromJsonAsync<List<SessionCourseResponse>>();
        courses.Should().ContainSingle(c => c.Id == cid);
    }

    [Fact]
    public async Task GetCourses_NonExistentSession_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/9999/courses");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReplaceCourses_DraftSession_ReturnsOk()
    {
        var cid1 = await CreateCourse("LOG600");
        var cid2 = await CreateCourse("LOG700");
        var session = await CreateDraftSession("Replace");

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{session.Id}/courses",
            new UpdateSessionCoursesRequest(new List<int> { cid1, cid2 }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var courses = await response.Content.ReadFromJsonAsync<List<SessionCourseResponse>>();
        courses.Should().HaveCount(2);
    }

    [Fact]
    public async Task ReplaceCourses_OpenSession_ReturnsOk()
    {
        var cid = await CreateCourse("LOG800");
        var session = await CreateDraftSession("OpenReplace");

        await _client.PostAsync($"{BaseUrl}/{session.Id}/open", null);

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{session.Id}/courses",
            new UpdateSessionCoursesRequest(new List<int> { cid }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var courses = await response.Content.ReadFromJsonAsync<List<SessionCourseResponse>>();
        courses.Should().ContainSingle(c => c.Id == cid);
    }

    [Fact]
    public async Task ReplaceCourses_ClosedSession_ReturnsConflict()
    {
        var session = await CreateDraftSession("ClosedReplace");

        await _client.PostAsync($"{BaseUrl}/{session.Id}/open", null);
        await _client.PostAsync($"{BaseUrl}/{session.Id}/close", null);

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{session.Id}/courses",
            new UpdateSessionCoursesRequest(new List<int> { 1 }));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ReplaceCourses_NonExistentSession_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/9999/courses",
            new UpdateSessionCoursesRequest(new List<int> { 1 }));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
    public async Task Open_AlreadyOpenSession_ReturnsConflict()
    {
        var create = new CreateSessionRequest("AlreadyOpen", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        await _client.PostAsync($"{BaseUrl}/{created!.Id}/open", null);
        var response = await _client.PostAsync($"{BaseUrl}/{created.Id}/open", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Close_DraftSession_ReturnsConflict()
    {
        var create = new CreateSessionRequest("DraftClose", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var response = await _client.PostAsync($"{BaseUrl}/{created!.Id}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Archive_DraftSession_ReturnsConflict()
    {
        var create = new CreateSessionRequest("DraftArchive", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var response = await _client.PostAsync($"{BaseUrl}/{created!.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Archive_OpenSession_ReturnsConflict()
    {
        var create = new CreateSessionRequest("OpenArchive", DateTime.UtcNow, DateTime.UtcNow.AddMonths(4));
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, create);
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        await _client.PostAsync($"{BaseUrl}/{created!.Id}/open", null);
        var response = await _client.PostAsync($"{BaseUrl}/{created.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Open_NonExistentSession_ReturnsNotFound()
    {
        var response = await _client.PostAsync($"{BaseUrl}/99999/open", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Close_NonExistentSession_ReturnsNotFound()
    {
        var response = await _client.PostAsync($"{BaseUrl}/99999/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Archive_NonExistentSession_ReturnsNotFound()
    {
        var response = await _client.PostAsync($"{BaseUrl}/99999/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Export CSV Tests

    [Fact]
    public async Task ExportCsv_ExistingSession_ReturnsFile()
    {
        var session = await CreateDraftSession("Export Test");

        var response = await _client.GetAsync($"{BaseUrl}/{session.Id}/export");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    }

    [Fact]
    public async Task ExportCsv_NonExistentSession_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"{BaseUrl}/99999/export");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExportCsv_FileNameContainsSessionTitle()
    {
        var session = await CreateDraftSession("Automne 2026");

        var response = await _client.GetAsync($"{BaseUrl}/{session.Id}/export");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var disposition = response.Content.Headers.ContentDisposition;
        disposition.Should().NotBeNull();
        disposition!.FileNameStar.Should().Contain("installations_").And.Contain("Automne");
    }

    #endregion
}
