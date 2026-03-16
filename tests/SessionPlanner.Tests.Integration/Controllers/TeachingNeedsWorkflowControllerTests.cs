using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Courses;
using SessionPlanner.Api.Dtos.Sessions;
using SessionPlanner.Api.Dtos.TeachingNeeds;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class TeachingNeedsWorkflowControllerTests : IClassFixture<TeachingNeedWorkflowWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TeachingNeedsWorkflowControllerTests(TeachingNeedWorkflowWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private void SetRole(string role)
    {
        _client.DefaultRequestHeaders.Remove("x-test-role");
        _client.DefaultRequestHeaders.Add("x-test-role", role);
    }

    private async Task<int> CreateCourseAsync(string code)
    {
        SetRole("admin");
        var response = await _client.PostAsJsonAsync("/api/v1/Courses", new CreateCourseRequest(code, "Workflow Course"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var course = await response.Content.ReadFromJsonAsync<CourseResponse>();
        return course!.Id;
    }

    private async Task<int> CreateOpenSessionAsync(string title)
    {
        SetRole("admin");
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/Sessions",
            new CreateSessionRequest(title, DateTime.UtcNow, DateTime.UtcNow.AddMonths(4)));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var session = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var openResponse = await _client.PostAsync($"/api/v1/Sessions/{session!.Id}/open", null);
        openResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        return session.Id;
    }

    private async Task<TeachingNeedResponse> CreateNeedAsTeacherAsync(int sessionId, int courseId)
    {
        SetRole("teacher");
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs",
            new CreateTeachingNeedRequest(courseId, null, "workflow need"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<TeachingNeedResponse>())!;
    }

    [Fact]
    public async Task Workflow_Approve_Path_EndToEnd_Works()
    {
        var sessionId = await CreateOpenSessionAsync("Workflow Approve");
        var courseId = await CreateCourseAsync("WF201");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("teacher");
        var submitResponse = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null);
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var submitted = await submitResponse.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        submitted!.Status.Should().Be("Submitted");

        SetRole("admin");
        var reviewResponse = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null);
        reviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviewed = await reviewResponse.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        reviewed!.Status.Should().Be("UnderReview");

        var approveResponse = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var approved = await approveResponse.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        approved!.Status.Should().Be("Approved");
    }

    [Fact]
    public async Task Workflow_Reject_Then_Revise_EndToEnd_Works()
    {
        var sessionId = await CreateOpenSessionAsync("Workflow Reject");
        var courseId = await CreateCourseAsync("WF202");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("teacher");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var rejectResponse = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/reject",
            new RejectTeachingNeedRequest("Please specify exact software version"));
        rejectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var rejected = await rejectResponse.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        rejected!.Status.Should().Be("Rejected");
        rejected.RejectionReason.Should().Be("Please specify exact software version");

        SetRole("teacher");
        var reviseResponse = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/revise", null);
        reviseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var revised = await reviseResponse.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        revised!.Status.Should().Be("Draft");
        revised.RejectionReason.Should().BeNull();
    }

    [Fact]
    public async Task Reject_WithoutReason_Returns400()
    {
        var sessionId = await CreateOpenSessionAsync("Reject Missing Reason");
        var courseId = await CreateCourseAsync("WF203");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("teacher");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var rejectResponse = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/reject",
            new RejectTeachingNeedRequest("   "));

        rejectResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvalidTransition_Returns409()
    {
        var sessionId = await CreateOpenSessionAsync("Invalid Transition");
        var courseId = await CreateCourseAsync("WF204");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("admin");
        var response = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UnauthorizedRole_Returns403()
    {
        var sessionId = await CreateOpenSessionAsync("Unauthorized Role");
        var courseId = await CreateCourseAsync("WF205");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("admin");
        var submitAsAdmin = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null);
        submitAsAdmin.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        SetRole("teacher");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var reviewAsTeacher = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null);
        reviewAsTeacher.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
