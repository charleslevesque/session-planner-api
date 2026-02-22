using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Softwares;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class SoftwaresControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/Softwares";

    public SoftwaresControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region GET Tests

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyList()
    {
        var response = await _client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var softwares = await response.Content.ReadFromJsonAsync<List<SoftwareResponse>>();
        softwares.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_AfterCreate_ReturnsSoftwares()
    {
        var request = new CreateSoftwareRequest("Visual Studio Code");
        await _client.PostAsJsonAsync(BaseUrl, request);

        var response = await _client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var softwares = await response.Content.ReadFromJsonAsync<List<SoftwareResponse>>();
        softwares.Should().NotBeNull();
        softwares.Should().Contain(s => s.Name == "Visual Studio Code");
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedSoftware()
    {
        var request = new CreateSoftwareRequest("JetBrains IntelliJ IDEA");

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var software = await response.Content.ReadFromJsonAsync<SoftwareResponse>();
        software.Should().NotBeNull();
        software!.Name.Should().Be("JetBrains IntelliJ IDEA");
        software.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Adobe Photoshop")]
    [InlineData("Microsoft Office 365")]
    [InlineData("AutoCAD 2025")]
    [InlineData("MATLAB R2025a")]
    public async Task Create_WithVariousNames_Succeeds(string name)
    {
        var request = new CreateSoftwareRequest(name);

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var software = await response.Content.ReadFromJsonAsync<SoftwareResponse>();
        software!.Name.Should().Be(name);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsNoContent()
    {
        // Arrange - Create a software first
        var createRequest = new CreateSoftwareRequest("Old Software Name");
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<SoftwareResponse>();

        var updateRequest = new UpdateSoftwareRequest("Updated Software Name");

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(BaseUrl);
        var softwares = await getResponse.Content.ReadFromJsonAsync<List<SoftwareResponse>>();
        softwares.Should().Contain(s => s.Name == "Updated Software Name");
    }

    [Fact]
    public async Task Update_WhenNotExists_ReturnsNotFound()
    {
        var updateRequest = new UpdateSoftwareRequest("Updated Name");

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/99999", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        var createRequest = new CreateSoftwareRequest("Software ToDelete");
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<SoftwareResponse>();

        var response = await _client.DeleteAsync($"{BaseUrl}/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WhenNotExists_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
