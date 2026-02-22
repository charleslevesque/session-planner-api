using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Dtos.Workstations;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class LaboratoriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/Laboratories";

    public LaboratoriesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region GET Tests

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyList()
    {
        var response = await _client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var labs = await response.Content.ReadFromJsonAsync<List<LaboratoryResponse>>();
        labs.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_WhenNotExists_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"{BaseUrl}/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedLaboratory()
    {
        var request = new CreateLaboratoryRequest("Lab Test-001", "Building Test", 25, 30);

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var lab = await response.Content.ReadFromJsonAsync<LaboratoryResponse>();
        lab.Should().NotBeNull();
        lab!.Name.Should().Be("Lab Test-001");
        lab.Building.Should().Be("Building Test");
        lab.NumberOfPCs.Should().Be(25);
        lab.SeatingCapacity.Should().Be(30);
        lab.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_ThenGetById_ReturnsCreatedLaboratory()
    {
        var request = new CreateLaboratoryRequest("Lab Test-002", "Building B", 20, 25);

        var createResponse = await _client.PostAsJsonAsync(BaseUrl, request);
        var created = await createResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();

        var getResponse = await _client.GetAsync($"{BaseUrl}/{created!.Id}");
        var retrieved = await getResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(created.Id);
        retrieved.Name.Should().Be("Lab Test-002");
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsNoContent()
    {
        var createRequest = new CreateLaboratoryRequest("Lab Original", "Building Original", 10, 15);
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();

        var updateRequest = new UpdateLaboratoryRequest("Lab Updated", "Building Updated", 50, 55);

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"{BaseUrl}/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();
        updated!.Name.Should().Be("Lab Updated");
        updated.Building.Should().Be("Building Updated");
    }

    [Fact]
    public async Task Update_WhenNotExists_ReturnsNotFound()
    {
        var updateRequest = new UpdateLaboratoryRequest("Lab Updated", "Building Updated", 50, 55);

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/999", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        var createRequest = new CreateLaboratoryRequest("Lab ToDelete", "Building X", 5, 10);
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();

        var response = await _client.DeleteAsync($"{BaseUrl}/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"{BaseUrl}/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenNotExists_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task GetAll_WithBuildingFilter_ReturnsFilteredResults()
    {
        await _client.PostAsJsonAsync(BaseUrl, new CreateLaboratoryRequest("Lab Filter1", "BuildingA", 10, 15));
        await _client.PostAsJsonAsync(BaseUrl, new CreateLaboratoryRequest("Lab Filter2", "BuildingB", 20, 25));

        var response = await _client.GetAsync($"{BaseUrl}?building=BuildingA");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var labs = await response.Content.ReadFromJsonAsync<List<LaboratoryResponse>>();
        labs.Should().OnlyContain(l => l.Building == "BuildingA");
    }

    [Fact]
    public async Task GetAll_WithMinCapacityFilter_ReturnsFilteredResults()
    {
        await _client.PostAsJsonAsync(BaseUrl, new CreateLaboratoryRequest("Lab MinCap1", "Building", 10, 15));
        await _client.PostAsJsonAsync(BaseUrl, new CreateLaboratoryRequest("Lab MinCap2", "Building", 20, 50));

        var response = await _client.GetAsync($"{BaseUrl}?minCapacity=40");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var labs = await response.Content.ReadFromJsonAsync<List<LaboratoryResponse>>();
        labs.Should().OnlyContain(l => l.SeatingCapacity >= 40);
    }

    [Fact]
    public async Task GetAll_WithMaxCapacityFilter_ReturnsFilteredResults()
    {
        await _client.PostAsJsonAsync(BaseUrl, new CreateLaboratoryRequest("Lab MaxCap1", "Building", 10, 10));
        await _client.PostAsJsonAsync(BaseUrl, new CreateLaboratoryRequest("Lab MaxCap2", "Building", 20, 100));

        var response = await _client.GetAsync($"{BaseUrl}?maxCapacity=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var labs = await response.Content.ReadFromJsonAsync<List<LaboratoryResponse>>();
        labs.Should().OnlyContain(l => l.SeatingCapacity <= 20);
    }

    [Fact]
    public async Task GetAll_WithCombinedFilters_ReturnsFilteredResults()
    {
        await _client.PostAsJsonAsync(BaseUrl, new CreateLaboratoryRequest("Lab Combined1", "FilterBuilding", 10, 25));
        await _client.PostAsJsonAsync(BaseUrl, new CreateLaboratoryRequest("Lab Combined2", "FilterBuilding", 20, 75));
        await _client.PostAsJsonAsync(BaseUrl, new CreateLaboratoryRequest("Lab Combined3", "OtherBuilding", 30, 50));

        var response = await _client.GetAsync($"{BaseUrl}?building=FilterBuilding&minCapacity=20&maxCapacity=30");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var labs = await response.Content.ReadFromJsonAsync<List<LaboratoryResponse>>();
        labs.Should().OnlyContain(l => l.Building == "FilterBuilding" && l.SeatingCapacity >= 20 && l.SeatingCapacity <= 30);
    }

    #endregion

    #region Workstation Tests

    [Fact]
    public async Task AddWorkstation_WithValidData_ReturnsCreatedWorkstation()
    {
        var labRequest = new CreateLaboratoryRequest("Lab Workstation", "Building WS", 30, 35);
        var labResponse = await _client.PostAsJsonAsync(BaseUrl, labRequest);
        var lab = await labResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();

        var osResponse = await _client.PostAsJsonAsync("/api/v1/OperatingSystems", 
            new { Name = "Windows 11 WS", Version = "23H2" });
        var os = await osResponse.Content.ReadFromJsonAsync<dynamic>();
        int osId = (int)os!.GetProperty("id").GetInt32();

        var wsRequest = new AddWorkstationRequest(osId, 10);
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/{lab!.Id}/workstations", wsRequest);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddWorkstation_ToNonExistingLab_ReturnsNotFound()
    {
        var wsRequest = new AddWorkstationRequest(1, 10);
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/99999/workstations", wsRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddWorkstation_WithNonExistingOS_ReturnsBadRequest()
    {
        var labRequest = new CreateLaboratoryRequest("Lab InvalidOS", "Building IO", 10, 15);
        var labResponse = await _client.PostAsJsonAsync(BaseUrl, labRequest);
        var lab = await labResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();

        var wsRequest = new AddWorkstationRequest(99999, 10);
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/{lab!.Id}/workstations", wsRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddWorkstation_ExistingOS_IncreasesCount()
    {
        var labRequest = new CreateLaboratoryRequest("Lab Existing WS", "Building EWS", 30, 35);
        var labResponse = await _client.PostAsJsonAsync(BaseUrl, labRequest);
        var lab = await labResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();

        var osResponse = await _client.PostAsJsonAsync("/api/v1/OperatingSystems", 
            new { Name = "Windows 10 EWS", Version = "22H2" });
        var os = await osResponse.Content.ReadFromJsonAsync<dynamic>();
        int osId = (int)os!.GetProperty("id").GetInt32();

        var wsRequest = new AddWorkstationRequest(osId, 5);
        await _client.PostAsJsonAsync($"{BaseUrl}/{lab!.Id}/workstations", wsRequest);
        var response = await _client.PostAsJsonAsync($"{BaseUrl}/{lab.Id}/workstations", wsRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ws = await response.Content.ReadFromJsonAsync<WorkstationResponse>();
        ws!.Count.Should().Be(10); // 5 + 5
    }

    [Fact]
    public async Task RemoveWorkstation_WhenExists_ReturnsNoContent()
    {
        var labRequest = new CreateLaboratoryRequest("Lab RemoveWS", "Building RWS", 20, 25);
        var labResponse = await _client.PostAsJsonAsync(BaseUrl, labRequest);
        var lab = await labResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();

        var osResponse = await _client.PostAsJsonAsync("/api/v1/OperatingSystems", 
            new { Name = "Ubuntu RWS", Version = "24.04" });
        var os = await osResponse.Content.ReadFromJsonAsync<dynamic>();
        int osId = (int)os!.GetProperty("id").GetInt32();


        var wsRequest = new AddWorkstationRequest(osId, 5);
        await _client.PostAsJsonAsync($"{BaseUrl}/{lab!.Id}/workstations", wsRequest);


        var response = await _client.DeleteAsync($"{BaseUrl}/{lab.Id}/workstations/{osId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveWorkstation_FromNonExistingLab_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/99999/workstations/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveWorkstation_NonExistingWorkstation_ReturnsNotFound()
    {

        var labRequest = new CreateLaboratoryRequest("Lab NoWS", "Building NWS", 10, 15);
        var labResponse = await _client.PostAsJsonAsync(BaseUrl, labRequest);
        var lab = await labResponse.Content.ReadFromJsonAsync<LaboratoryResponse>();

        var response = await _client.DeleteAsync($"{BaseUrl}/{lab!.Id}/workstations/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
