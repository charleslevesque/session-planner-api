using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Courses;
using SessionPlanner.Api.Dtos.Sessions;
using SessionPlanner.Api.Dtos.TeachingNeeds;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class TeachingNeedsControllerTests : IClassFixture<TeachingNeedWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TeachingNeedsControllerTests(TeachingNeedWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ------------------------------------------------------------------ helpers

    private async Task<int> CreateCourseAsync(string code = "TST100")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/Courses",
            new CreateCourseRequest(code, "Test Course"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var course = await response.Content.ReadFromJsonAsync<CourseResponse>();
        return course!.Id;
    }

    private async Task<int> CreateDraftSessionAsync(string title = "Session Test")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/Sessions",
            new CreateSessionRequest(title, DateTime.UtcNow, DateTime.UtcNow.AddMonths(4)));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        return session!.Id;
    }

    private async Task<int> CreateOpenSessionAsync(string title = "Session Ouverte")
    {
        var sessionId = await CreateDraftSessionAsync(title);
        var openResponse = await _client.PostAsync($"/api/v1/Sessions/{sessionId}/open", null);
        openResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return sessionId;
    }

    private async Task<TeachingNeedResponse> CreateNeedAsync(int sessionId, int courseId)
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs",
            new CreateTeachingNeedRequest(courseId, TeachingNeedWebApplicationFactory.SeededPersonnelId, null, null, null, null, null, null, null));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<TeachingNeedResponse>())!;
    }

    private static string NeedsUrl(int sessionId) => $"/api/v1/sessions/{sessionId}/needs";
    private static string NeedUrl(int sessionId, int needId) => $"/api/v1/sessions/{sessionId}/needs/{needId}";
    private static string ItemsUrl(int sessionId, int needId) => $"/api/v1/sessions/{sessionId}/needs/{needId}/items";
    private static string ItemUrl(int sessionId, int needId, int itemId) => $"/api/v1/sessions/{sessionId}/needs/{needId}/items/{itemId}";

    // ------------------------------------------------------------------ GET all

    #region GET /sessions/{sessionId}/needs

    [Fact]
    public async Task GetAll_WhenNoNeeds_ReturnsOkWithEmptyList()
    {
        var sessionId = await CreateOpenSessionAsync("GetAll Empty");

        var response = await _client.GetAsync(NeedsUrl(sessionId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var needs = await response.Content.ReadFromJsonAsync<List<TeachingNeedResponse>>();
        needs.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetAll_AfterCreate_ReturnsNeeds()
    {
        var sessionId = await CreateOpenSessionAsync("GetAll Populated");
        var courseId = await CreateCourseAsync("TST101");
        await CreateNeedAsync(sessionId, courseId);

        var response = await _client.GetAsync(NeedsUrl(sessionId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var needs = await response.Content.ReadFromJsonAsync<List<TeachingNeedResponse>>();
        needs.Should().HaveCount(1);
        needs![0].CourseId.Should().Be(courseId);
        needs[0].PersonnelId.Should().Be(TeachingNeedWebApplicationFactory.SeededPersonnelId);
    }

    #endregion

    // ------------------------------------------------------------------ GET by id

    #region GET /sessions/{sessionId}/needs/{id}

    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        var sessionId = await CreateOpenSessionAsync("GetById 404");

        var response = await _client.GetAsync(NeedUrl(sessionId, 9999));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ExistingNeed_ReturnsNeed()
    {
        var sessionId = await CreateOpenSessionAsync("GetById OK");
        var courseId = await CreateCourseAsync("TST102");
        var created = await CreateNeedAsync(sessionId, courseId);

        var response = await _client.GetAsync(NeedUrl(sessionId, created.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var need = await response.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        need.Should().NotBeNull();
        need!.Id.Should().Be(created.Id);
        need.SessionId.Should().Be(sessionId);
        need.CourseId.Should().Be(courseId);
        need.Status.Should().Be("Draft");
    }

    #endregion

    // ------------------------------------------------------------------ POST (create)

    #region POST /sessions/{sessionId}/needs

    [Fact]
    public async Task Create_WithNonExistentSession_ReturnsConflict()
    {
        var courseId = await CreateCourseAsync("TST103");

        var response = await _client.PostAsJsonAsync(
            NeedsUrl(99999),
            new CreateTeachingNeedRequest(courseId, TeachingNeedWebApplicationFactory.SeededPersonnelId, null, null, null, null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithDraftSession_ReturnsConflict()
    {
        var sessionId = await CreateDraftSessionAsync("Draft Session For Needs");
        var courseId = await CreateCourseAsync("TST104");

        var response = await _client.PostAsJsonAsync(
            NeedsUrl(sessionId),
            new CreateTeachingNeedRequest(courseId, TeachingNeedWebApplicationFactory.SeededPersonnelId, null, null, null, null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithOpenSession_ReturnsCreated()
    {
        var sessionId = await CreateOpenSessionAsync("Open Session For Needs");
        var courseId = await CreateCourseAsync("TST105");

        var response = await _client.PostAsJsonAsync(
            NeedsUrl(sessionId),
            new CreateTeachingNeedRequest(courseId, TeachingNeedWebApplicationFactory.SeededPersonnelId, "Besoin de Java 21", null, null, null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var need = await response.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        need.Should().NotBeNull();
        need!.SessionId.Should().Be(sessionId);
        need.CourseId.Should().Be(courseId);
        need.PersonnelId.Should().Be(TeachingNeedWebApplicationFactory.SeededPersonnelId);
        need.Notes.Should().Be("Besoin de Java 21");
        need.Status.Should().Be("Draft");
        need.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_WithClosedSession_ReturnsConflict()
    {
        var sessionId = await CreateOpenSessionAsync("Close then Create");
        var courseId = await CreateCourseAsync("TST106");
        await _client.PostAsync($"/api/v1/Sessions/{sessionId}/close", null);

        var response = await _client.PostAsJsonAsync(
            NeedsUrl(sessionId),
            new CreateTeachingNeedRequest(courseId, TeachingNeedWebApplicationFactory.SeededPersonnelId, null, null, null, null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_AdminWithoutPersonnelId_ReturnsBadRequest()
    {
        var sessionId = await CreateOpenSessionAsync("No PersonnelId");
        var courseId = await CreateCourseAsync("TST107");

        // Admin path requires PersonnelId — omit it
        var response = await _client.PostAsJsonAsync(
            NeedsUrl(sessionId),
            new CreateTeachingNeedRequest(courseId, null, null, null, null, null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    // ------------------------------------------------------------------ PUT (update)

    #region PUT /sessions/{sessionId}/needs/{id}

    [Fact]
    public async Task Update_WhenNotFound_Returns404()
    {
        var sessionId = await CreateOpenSessionAsync("Update 404");
        var courseId = await CreateCourseAsync("TST108");

        var response = await _client.PutAsJsonAsync(
            NeedUrl(sessionId, 9999),
            new UpdateTeachingNeedRequest(courseId, null, null, null, null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_DraftNeed_ReturnsOk()
    {
        var sessionId = await CreateOpenSessionAsync("Update Draft");
        var courseId1 = await CreateCourseAsync("TST109A");
        var courseId2 = await CreateCourseAsync("TST109B");
        var created = await CreateNeedAsync(sessionId, courseId1);

        var response = await _client.PutAsJsonAsync(
            NeedUrl(sessionId, created.Id),
            new UpdateTeachingNeedRequest(courseId2, "Commentaire modifié", null, null, null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        updated!.CourseId.Should().Be(courseId2);
        updated.Notes.Should().Be("Commentaire modifié");
    }

    #endregion

    // ------------------------------------------------------------------ DELETE (need)

    #region DELETE /sessions/{sessionId}/needs/{id}

    [Fact]
    public async Task Delete_WhenNotFound_Returns404()
    {
        var sessionId = await CreateOpenSessionAsync("Delete 404");

        var response = await _client.DeleteAsync(NeedUrl(sessionId, 9999));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_DraftNeed_ReturnsNoContent()
    {
        var sessionId = await CreateOpenSessionAsync("Delete Draft");
        var courseId = await CreateCourseAsync("TST110");
        var created = await CreateNeedAsync(sessionId, courseId);

        var response = await _client.DeleteAsync(NeedUrl(sessionId, created.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_AlreadyDeleted_Returns404()
    {
        var sessionId = await CreateOpenSessionAsync("Double Delete");
        var courseId = await CreateCourseAsync("TST111");
        var created = await CreateNeedAsync(sessionId, courseId);

        await _client.DeleteAsync(NeedUrl(sessionId, created.Id));
        var response = await _client.DeleteAsync(NeedUrl(sessionId, created.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    // ------------------------------------------------------------------ POST items

    #region POST /sessions/{sessionId}/needs/{id}/items

    [Fact]
    public async Task AddItem_WhenNeedNotFound_Returns404()
    {
        var sessionId = await CreateOpenSessionAsync("AddItem 404");

        var response = await _client.PostAsJsonAsync(
            ItemsUrl(sessionId, 9999),
            new AddNeedItemRequest(null, null, null, null, 30, null, "Notes", null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddItem_ToDraftNeed_Returns201WithItem()
    {
        var sessionId = await CreateOpenSessionAsync("AddItem Draft");
        var courseId = await CreateCourseAsync("TST112");
        var need = await CreateNeedAsync(sessionId, courseId);

        var response = await _client.PostAsJsonAsync(
            ItemsUrl(sessionId, need.Id),
            new AddNeedItemRequest(null, null, null, null, 25, null, "Poste supplémentaire", null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await response.Content.ReadFromJsonAsync<TeachingNeedItemResponse>();
        item.Should().NotBeNull();
        item!.Quantity.Should().Be(25);
        item.Notes.Should().Be("Poste supplémentaire");
    }

    [Fact]
    public async Task AddItem_AppearsInGetById()
    {
        var sessionId = await CreateOpenSessionAsync("AddItem Appears");
        var courseId = await CreateCourseAsync("TST113");
        var need = await CreateNeedAsync(sessionId, courseId);

        await _client.PostAsJsonAsync(
            ItemsUrl(sessionId, need.Id),
            new AddNeedItemRequest(null, null, null, null, 10, null, "item test", null));

        var response = await _client.GetAsync(NeedUrl(sessionId, need.Id));
        var updated = await response.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        updated!.Items.Should().HaveCount(1);
        updated.Items.First().Quantity.Should().Be(10);
    }

    #endregion

    // ------------------------------------------------------------------ DELETE items

    #region DELETE /sessions/{sessionId}/needs/{id}/items/{itemId}

    [Fact]
    public async Task RemoveItem_WhenNeedNotFound_Returns404()
    {
        var sessionId = await CreateOpenSessionAsync("RemoveItem Need 404");

        var response = await _client.DeleteAsync(ItemUrl(sessionId, 9999, 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveItem_WhenItemNotFound_Returns404()
    {
        var sessionId = await CreateOpenSessionAsync("RemoveItem Item 404");
        var courseId = await CreateCourseAsync("TST114");
        var need = await CreateNeedAsync(sessionId, courseId);

        var response = await _client.DeleteAsync(ItemUrl(sessionId, need.Id, 9999));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveItem_ExistingItem_ReturnsNoContent()
    {
        var sessionId = await CreateOpenSessionAsync("RemoveItem OK");
        var courseId = await CreateCourseAsync("TST115");
        var need = await CreateNeedAsync(sessionId, courseId);

        var addResponse = await _client.PostAsJsonAsync(
            ItemsUrl(sessionId, need.Id),
            new AddNeedItemRequest(null, null, null, null, 5, null, null, null));
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await addResponse.Content.ReadFromJsonAsync<TeachingNeedItemResponse>();

        var deleteResponse = await _client.DeleteAsync(ItemUrl(sessionId, need.Id, item!.Id));

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveItem_ItemGoneAfterDelete()
    {
        var sessionId = await CreateOpenSessionAsync("RemoveItem Gone");
        var courseId = await CreateCourseAsync("TST116");
        var need = await CreateNeedAsync(sessionId, courseId);

        var addResponse = await _client.PostAsJsonAsync(
            ItemsUrl(sessionId, need.Id),
            new AddNeedItemRequest(null, null, null, null, 8, null, null, null));
        var item = await addResponse.Content.ReadFromJsonAsync<TeachingNeedItemResponse>();

        await _client.DeleteAsync(ItemUrl(sessionId, need.Id, item!.Id));

        var getResponse = await _client.GetAsync(NeedUrl(sessionId, need.Id));
        var updated = await getResponse.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        updated!.Items.Should().BeEmpty();
    }

    #endregion
}
