using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.CourseResources;
using SessionPlanner.Api.Dtos.Courses;
using SessionPlanner.Api.Dtos.Sessions;
using SessionPlanner.Api.Dtos.Softwares;
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
        SetRole("professor");
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs",
            new CreateTeachingNeedRequest(courseId, null, "workflow need", null, null, null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<TeachingNeedResponse>())!;
    }

    private async Task<int> CreateSoftwareAsync(string name)
    {
        SetRole("admin");
        var response = await _client.PostAsJsonAsync("/api/v1/softwares", new { name });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<SoftwareResponse>();
        return created!.Id;
    }

    [Fact]
    public async Task Workflow_Approve_Path_EndToEnd_Works()
    {
        var sessionId = await CreateOpenSessionAsync("Workflow Approve");
        var courseId = await CreateCourseAsync("WF201");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        var submitResponse = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null);
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var submitted = await submitResponse.Content.ReadFromJsonAsync<SubmitTeachingNeedResponse>();
        submitted!.Need.Status.Should().Be("Submitted");

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
    public async Task Submit_WithDifferentSoftwareVersionsInSameCourseAndSession_ReturnsNonBlockingConflictWarnings()
    {
        var sessionId = await CreateOpenSessionAsync("Workflow Conflict Warning");
        var courseId = await CreateCourseAsync("WFCF01");

        // Need #1: IntelliJ v1 submitted first (baseline in same course+session).
        var need1 = await CreateNeedAsTeacherAsync(sessionId, courseId);
        SetRole("professor");
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need1.Id}/items",
            new AddNeedItemRequest("software", 1, 1, 1, 1, null, null, null)))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var submitNeed1 = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need1.Id}/submit", null);
        submitNeed1.StatusCode.Should().Be(HttpStatusCode.OK);
        var submitNeed1Body = await submitNeed1.Content.ReadFromJsonAsync<SubmitTeachingNeedResponse>();
        submitNeed1Body.Should().NotBeNull();
        submitNeed1Body!.Need.Status.Should().Be("Submitted");
        submitNeed1Body.Warnings.Should().BeEmpty("first submission has no prior conflicting need");

        // Need #2: same software but different version should trigger warning (non-blocking).
        var need2 = await CreateNeedAsTeacherAsync(sessionId, courseId);
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need2.Id}/items",
            new AddNeedItemRequest("software", 1, 2, 1, 1, null, null, null)))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var submitNeed2 = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need2.Id}/submit", null);
        submitNeed2.StatusCode.Should().Be(HttpStatusCode.OK, "conflict detection is warning-only and must not block submit");
        var submitNeed2Body = await submitNeed2.Content.ReadFromJsonAsync<SubmitTeachingNeedResponse>();
        submitNeed2Body.Should().NotBeNull();
        submitNeed2Body!.Need.Status.Should().Be("Submitted");
        submitNeed2Body.Warnings.Should().NotBeEmpty();
        submitNeed2Body.Warnings.Should().Contain(w =>
            w.Contains("Conflit:", StringComparison.OrdinalIgnoreCase)
            && w.Contains("IntelliJ", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Workflow_Approve_Propagates_SoftwareVersion_To_Course_Resources()
    {
        var sessionId = await CreateOpenSessionAsync("Workflow Approve Propagate");
        var courseId = await CreateCourseAsync("WFPROP");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        var details = JsonSerializer.Serialize(new
        {
            softwareName = "WorkflowTool",
            versionNumber = "2.1",
            osId = "1",
            installationDetails = (string?)null,
            notes = (string?)null,
        });
        var addItemResponse = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items",
            new AddNeedItemRequest("software", null, null, null, null, null, null, details));
        addItemResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var resourcesResponse = await _client.GetAsync($"/api/v1/Courses/{courseId}/resources");
        resourcesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var resources = await resourcesResponse.Content.ReadFromJsonAsync<CourseResourcesResponse>();
        resources!.SoftwareVersionIds.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Workflow_Reject_Then_Revise_EndToEnd_Works()
    {
        var sessionId = await CreateOpenSessionAsync("Workflow Reject");
        var courseId = await CreateCourseAsync("WF202");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
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

        SetRole("professor");
        var reviseResponse = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/revise", null);
        reviseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var revised = await reviseResponse.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        revised!.Status.Should().Be("Draft");
        revised.RejectionReason.Should().BeNull();
        revised.ReviewedAt.Should().BeNull();
        revised.ReviewedByUserId.Should().BeNull();

        var submitAgain = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null);
        submitAgain.StatusCode.Should().Be(HttpStatusCode.OK);
        var resubmitted = await submitAgain.Content.ReadFromJsonAsync<SubmitTeachingNeedResponse>();
        resubmitted!.Need.Status.Should().Be("Submitted");

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var finalApproved = await _client.GetAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}");
        finalApproved.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalBody = await finalApproved.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        finalBody!.Status.Should().Be("Approved");
    }

    [Fact]
    public async Task Workflow_Reject_MyNeeds_Filter_And_RejectionReason()
    {
        var sessionId = await CreateOpenSessionAsync("Mine Rejected");
        var courseId = await CreateCourseAsync("WF301");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var rejectResponse = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/reject",
            new RejectTeachingNeedRequest("Motif test rejet"));
        rejectResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("professor");
        var mineRejected = await _client.GetAsync("/api/v1/needs/mine?status=rejected");
        mineRejected.StatusCode.Should().Be(HttpStatusCode.OK);
        var rejectedList = await mineRejected.Content.ReadFromJsonAsync<List<MyNeedResponse>>();
        rejectedList.Should().NotBeNull();
        rejectedList!.Should().ContainSingle();
        rejectedList[0].RejectionReason.Should().Be("Motif test rejet");

        var mineApproved = await _client.GetAsync("/api/v1/needs/mine?status=approved");
        mineApproved.StatusCode.Should().Be(HttpStatusCode.OK);
        var approvedList = await mineApproved.Content.ReadFromJsonAsync<List<MyNeedResponse>>();
        approvedList.Should().NotBeNull();
        approvedList!.Should().BeEmpty();
    }

    [Fact]
    public async Task Workflow_Rejected_Professor_Can_Add_And_Remove_Item()
    {
        var sessionId = await CreateOpenSessionAsync("Rejected Edit Items");
        var courseId = await CreateCourseAsync("WF302");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/reject",
            new RejectTeachingNeedRequest("Adjust items"))).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("professor");
        var addResponse = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items",
            new AddNeedItemRequest("other", null, null, null, null, "Extra", null, null));
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var addedItem = await addResponse.Content.ReadFromJsonAsync<TeachingNeedItemResponse>();

        var removeResponse = await _client.DeleteAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items/{addedItem!.Id}");
        removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Reject_WithoutReason_Returns400()
    {
        var sessionId = await CreateOpenSessionAsync("Reject Missing Reason");
        var courseId = await CreateCourseAsync("WF203");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
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
    public async Task Workflow_Approve_WithIncompleteSoftwareItem_Returns409_And_StatusStaysUnderReview()
    {
        var sessionId = await CreateOpenSessionAsync("Approve Strict Fail");
        var courseId = await CreateCourseAsync("WFSTRICT");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        // detailsJson is intentionally missing osId → propagation must throw → approval must be rolled back.
        var badDetails = JsonSerializer.Serialize(new { softwareName = "StrictTool", versionNumber = "9.0" });
        var addResp = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items",
            new AddNeedItemRequest("software", null, null, null, null, null, null, badDetails));
        addResp.StatusCode.Should().Be(HttpStatusCode.Created);

        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var approveResp = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null);
        approveResp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var getResp = await _client.GetAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var stillUnderReview = await getResp.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        stillUnderReview!.Status.Should().Be("UnderReview");
    }

    [Fact]
    public async Task Workflow_Approve_SaaS_FindOrCreate_NoDuplicate()
    {
        var sessionId = await CreateOpenSessionAsync("Approve SaaS dedup");
        var courseId = await CreateCourseAsync("WFSAAS");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        var saasDetails = JsonSerializer.Serialize(new { name = "SaaSToolDedup", numberOfAccounts = "10" });
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items",
            new AddNeedItemRequest("saas", null, null, null, null, null, null, saasDetails))).StatusCode.Should().Be(HttpStatusCode.Created);

        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Second need with identical SaaS — should reuse existing SaaS product.
        var need2 = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need2.Id}/items",
            new AddNeedItemRequest("saas", null, null, null, null, null, null, saasDetails))).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need2.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need2.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need2.Id}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var resourcesResponse = await _client.GetAsync($"/api/v1/Courses/{courseId}/resources");
        var resources = await resourcesResponse.Content.ReadFromJsonAsync<CourseResourcesResponse>();
        resources!.SaaS.Where(s => s.Name == "SaaSToolDedup").Should().HaveCount(1);
    }

    [Fact]
    public async Task Workflow_Approve_UnknownItemType_Returns409_And_StatusStaysUnderReview()
    {
        var sessionId = await CreateOpenSessionAsync("Approve Unknown Type Fail");
        var courseId = await CreateCourseAsync("WFUNK");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        var addResponse = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items",
            new AddNeedItemRequest("new_future_type", null, null, null, null, "unsupported item", null, null));
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var approveResp = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null);
        approveResp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var getResp = await _client.GetAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var stillUnderReview = await getResp.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        stillUnderReview!.Status.Should().Be("UnderReview");
    }

    [Fact]
    public async Task Workflow_Approve_Configuration_FindOrCreate_NoDuplicateBySameSpec()
    {
        var sessionId = await CreateOpenSessionAsync("Approve Configuration dedup");
        var courseId = await CreateCourseAsync("WFCONF");

        var details = JsonSerializer.Serialize(new
        {
            title = "Open port 8888",
            osIds = "1",
            laboratoryIds = "1",
            notes = "same config"
        });

        var need1 = await CreateNeedAsTeacherAsync(sessionId, courseId);
        SetRole("professor");
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need1.Id}/items",
            new AddNeedItemRequest("configuration", null, null, null, null, null, null, details))).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need1.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need1.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need1.Id}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var need2 = await CreateNeedAsTeacherAsync(sessionId, courseId);
        SetRole("professor");
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need2.Id}/items",
            new AddNeedItemRequest("configuration", null, null, null, null, null, null, details))).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need2.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need2.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need2.Id}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var resourcesResponse = await _client.GetAsync($"/api/v1/Courses/{courseId}/resources");
        resourcesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var resources = await resourcesResponse.Content.ReadFromJsonAsync<CourseResourcesResponse>();
        resources!.Configurations.Where(c => c.Title == "Open port 8888").Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMine_StatusFilter_UnderReview_NormalisedAlias()
    {
        var sessionId = await CreateOpenSessionAsync("Mine Filter UnderReview");
        var courseId = await CreateCourseAsync("WF_FILTER");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("professor");
        // Aliases: "under review", "underreview", "under_review" must all return the need created above.
        foreach (var alias in new[] { "under review", "underreview", "under_review" })
        {
            var resp = await _client.GetAsync(
                $"/api/v1/needs/mine?sessionId={sessionId}&status={Uri.EscapeDataString(alias)}");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await resp.Content.ReadFromJsonAsync<List<MyNeedResponse>>();
            list.Should().NotBeNull();
            list!.Should().ContainSingle(n => n.Id == need.Id, $"alias '{alias}' should match the UnderReview need");
        }
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

        SetRole("professor");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var reviewAsTeacher = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null);
        reviewAsTeacher.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Non-regression for the RAM 2→4 resubmission bug:
    /// When a teacher deletes a VM item (ramGb=2) and replaces it with a new one
    /// (ramGb=4) before resubmitting, the approval must propagate ramGb=4, not ramGb=2.
    /// </summary>
    [Fact]
    public async Task Workflow_Resubmission_DeleteAndReaddItem_CorrectValuePropagatedOnApproval()
    {
        var sessionId = await CreateOpenSessionAsync("Resubmission RAM Fix");
        var courseId = await CreateCourseAsync("WFRAM");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        // 1. Add a VM item with ramGb = 2.
        SetRole("professor");
        var details1 = JsonSerializer.Serialize(new
        {
            quantity = "1",
            cpuCores = "2",
            ramGb = "2",
            storageGb = "50",
            accessType = "SSH",
            osId = "1",
        });
        var addResp1 = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items",
            new AddNeedItemRequest("virtual_machine", null, null, null, null, null, null, details1));
        addResp1.StatusCode.Should().Be(HttpStatusCode.Created);
        var addedItem = await addResp1.Content.ReadFromJsonAsync<TeachingNeedItemResponse>();

        // 2. Submit → admin rejects.
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/reject",
            new RejectTeachingNeedRequest("Augmenter la RAM"))).StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Teacher: delete the ramGb=2 item, add a new one with ramGb=4.
        SetRole("professor");
        (await _client.DeleteAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items/{addedItem!.Id}"))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details2 = JsonSerializer.Serialize(new
        {
            quantity = "1",
            cpuCores = "2",
            ramGb = "4",
            storageGb = "50",
            accessType = "SSH",
            osId = "1",
        });
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items",
            new AddNeedItemRequest("virtual_machine", null, null, null, null, null, null, details2)))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        // 4. Revise → resubmit.
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/revise", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Admin approves.
        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. The course resource must have exactly one VM with ramGb=4 (not 2).
        var resourcesResp = await _client.GetAsync($"/api/v1/Courses/{courseId}/resources");
        resourcesResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var resources = await resourcesResp.Content.ReadFromJsonAsync<CourseResourcesResponse>();
        var vms = resources!.VirtualMachines.Where(v => v.RamGb == 4).ToList();
        vms.Should().HaveCount(1, "the updated VM (ramGb=4) must be propagated");
        resources.VirtualMachines.Should().NotContain(v => v.RamGb == 2,
            "the original VM (ramGb=2) must not have been propagated");
    }

    /// <summary>
    /// An approved need must remain fully readable via GET with all its items intact,
    /// providing a stable historical snapshot.
    /// </summary>
    [Fact]
    public async Task Workflow_Approved_NeedHistoryRemainsReadable()
    {
        var sessionId = await CreateOpenSessionAsync("Approved History Readable");
        var courseId = await CreateCourseAsync("WFHIST");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        var itemDetails = JsonSerializer.Serialize(new
        {
            name = "HistorySaaS",
            numberOfAccounts = "5",
        });
        (await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items",
            new AddNeedItemRequest("saas", null, null, null, null, null, null, itemDetails)))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        // The approved need must still be readable in full, including its items.
        var getResp = await _client.GetAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var approvedNeed = await getResp.Content.ReadFromJsonAsync<TeachingNeedResponse>();
        approvedNeed!.Status.Should().Be("Approved");
        approvedNeed.Items.Should().HaveCount(1, "the item snapshot must be preserved after approval");
        approvedNeed.Items.First().ItemType.Should().Be("saas");
    }

    // ------------------------------------------------------------------ One-Click Renewal

    #region One-Click Renewal

    private async Task<TeachingNeedResponse> CreateAndApproveNeedAsync(int sessionId, int courseId)
    {
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        var addItemResp = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{sessionId}/needs/{need.Id}/items",
            new AddNeedItemRequest("software", 1, 1, 1, null, null, null, null));
        addItemResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var submitResp = await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null);
        submitResp.StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var getResp = await _client.GetAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}");
        return (await getResp.Content.ReadFromJsonAsync<TeachingNeedResponse>())!;
    }

    [Fact]
    public async Task RenewableCourses_ReturnsCoursesWithApprovedHistoryOnly()
    {
        var session1 = await CreateOpenSessionAsync("Renew S1");
        var session2 = await CreateOpenSessionAsync("Renew S2");
        var courseA = await CreateCourseAsync("RENEW_A");
        var courseB = await CreateCourseAsync("RENEW_B");

        await CreateAndApproveNeedAsync(session1, courseA);
        await CreateNeedAsTeacherAsync(session1, courseB);

        SetRole("professor");
        var resp = await _client.GetAsync($"/api/v1/sessions/{session2}/needs/renewable-courses");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var renewables = await resp.Content.ReadFromJsonAsync<List<RenewableCourseResponse>>();
        renewables.Should().NotBeNull();
        renewables!.Should().Contain(r => r.CourseId == courseA,
            "courseA had an approved need in session1");
        renewables.Should().NotContain(r => r.CourseId == courseB,
            "courseB only had a draft need, not approved");
    }

    [Fact]
    public async Task Renew_CreatesNewDraftWithItems()
    {
        var session1 = await CreateOpenSessionAsync("Renew Clone S1");
        var session2 = await CreateOpenSessionAsync("Renew Clone S2");
        var courseId = await CreateCourseAsync("RENEW_C");

        var approved = await CreateAndApproveNeedAsync(session1, courseId);
        approved.Items.Should().HaveCount(1, "we added 1 item before approval");

        SetRole("professor");
        var renewResp = await _client.PostAsync(
            $"/api/v1/sessions/{session2}/needs/renew/{courseId}", null);
        renewResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await renewResp.Content.ReadFromJsonAsync<RenewNeedsResponse>();
        result.Should().NotBeNull();
        result!.Need.SessionId.Should().Be(session2);
        result.Need.CourseId.Should().Be(courseId);
        result.Need.Status.Should().Be("Draft");
        result.Need.Items.Should().HaveCount(1, "items are cloned from the source need");
        result.Changes.Should().NotBeEmpty("at least the 'renewed from' message should be present");
    }

    [Fact]
    public async Task Renew_WhenNoHistory_Returns409()
    {
        var session = await CreateOpenSessionAsync("Renew No History");
        var courseId = await CreateCourseAsync("RENEW_NONE");

        SetRole("professor");
        var resp = await _client.PostAsync($"/api/v1/sessions/{session}/needs/renew/{courseId}", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Renew_UpgradesSoftwareVersionToLatest()
    {
        var session1 = await CreateOpenSessionAsync("Renew Upgrade S1");
        var session2 = await CreateOpenSessionAsync("Renew Upgrade S2");
        var courseId = await CreateCourseAsync("RENEW_UPG");

        var need = await CreateNeedAsTeacherAsync(session1, courseId);

        SetRole("professor");
        var addItemResp = await _client.PostAsJsonAsync(
            $"/api/v1/sessions/{session1}/needs/{need.Id}/items",
            new AddNeedItemRequest("software", 1, 1, 1, null, null, null, null));
        addItemResp.StatusCode.Should().Be(HttpStatusCode.Created);

        (await _client.PostAsync($"/api/v1/sessions/{session1}/needs/{need.Id}/submit", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{session1}/needs/{need.Id}/review", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{session1}/needs/{need.Id}/approve", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("professor");
        var renewResp = await _client.PostAsync(
            $"/api/v1/sessions/{session2}/needs/renew/{courseId}", null);
        renewResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await renewResp.Content.ReadFromJsonAsync<RenewNeedsResponse>();
        result.Should().NotBeNull();

        var item = result!.Need.Items.First();
        item.SoftwareId.Should().Be(1);
        item.SoftwareVersionId.Should().Be(2, "v2 is the latest version seeded for software=1");
    }

    #endregion

    /// <summary>
    /// Approved needs must not appear in the active-needs list when
    /// the teacher requests mine?status=approved (it should be empty
    /// for a freshly-approved need that was never re-opened).
    /// This guards against the badge-counter counting Approved as active.
    /// </summary>
    [Fact]
    public async Task Workflow_Approved_NotCountedAsActiveNeed()
    {
        var sessionId = await CreateOpenSessionAsync("Approved Not Active");
        var courseId = await CreateCourseAsync("WFACT");
        var need = await CreateNeedAsTeacherAsync(sessionId, courseId);

        SetRole("professor");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/submit", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        SetRole("admin");
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/review", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/v1/sessions/{sessionId}/needs/{need.Id}/approve", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        // Active statuses (Draft / Submitted / UnderReview / Rejected) must be empty.
        SetRole("professor");
        foreach (var activeStatus in new[] { "draft", "submitted", "under review", "rejected" })
        {
            var resp = await _client.GetAsync(
                $"/api/v1/needs/mine?sessionId={sessionId}&status={Uri.EscapeDataString(activeStatus)}");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await resp.Content.ReadFromJsonAsync<List<MyNeedResponse>>();
            list.Should().NotBeNull();
            list!.Should().NotContain(
                n => n.Id == need.Id,
                $"Approved need must not appear in '{activeStatus}' active filter");
        }

        // Querying with status=approved must return the need.
        var approvedResp = await _client.GetAsync(
            $"/api/v1/needs/mine?sessionId={sessionId}&status=approved");
        approvedResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var approvedList = await approvedResp.Content.ReadFromJsonAsync<List<MyNeedResponse>>();
        approvedList.Should().ContainSingle(n => n.Id == need.Id,
            "Approved need must appear when filtering by status=approved");
    }
}
