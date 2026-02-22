using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Softwares;
using SessionPlanner.Api.Dtos.SoftwareVersions;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class SoftwareVersionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/SoftwareVersions";

    public SoftwareVersionsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> CreateSoftwareAsync(string name = "Test Software")
    {
        var request = new CreateSoftwareRequest(name);
        var response = await _client.PostAsJsonAsync("/api/v1/Softwares", request);
        var software = await response.Content.ReadFromJsonAsync<SoftwareResponse>();
        return software!.Id;
    }

    #region GET Tests

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyList()
    {
        var response = await _client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var versions = await response.Content.ReadFromJsonAsync<List<SoftwareVersionResponse>>();
        versions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_AfterCreate_ReturnsVersions()
    {
        var softwareId = await CreateSoftwareAsync("Software for GetAll");
        var request = new CreateSoftwareVersionRequest(softwareId, 0, "1.0.0", null, null);
        await _client.PostAsJsonAsync(BaseUrl, request);

        var response = await _client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var versions = await response.Content.ReadFromJsonAsync<List<SoftwareVersionResponse>>();
        versions.Should().NotBeNull();
        versions.Should().Contain(v => v.VersionNumber == "1.0.0");
    }

    [Fact]
    public async Task GetBySoftwareId_ReturnsFilteredVersions()
    {
        var softwareId = await CreateSoftwareAsync("Software for Filter");
        var request = new CreateSoftwareVersionRequest(softwareId, 0, "2.0.0", null, null);
        await _client.PostAsJsonAsync(BaseUrl, request);

        var response = await _client.GetAsync($"/api/v1/softwares/{softwareId}/SoftwareVersions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var versions = await response.Content.ReadFromJsonAsync<List<SoftwareVersionResponse>>();
        versions.Should().NotBeNull();
        versions.Should().OnlyContain(v => v.SoftwareId == softwareId);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedVersion()
    {
        var softwareId = await CreateSoftwareAsync("Software for Create");
        var request = new CreateSoftwareVersionRequest(softwareId, 0, "3.0.0", "Install via MSI", "Initial release");

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var version = await response.Content.ReadFromJsonAsync<SoftwareVersionResponse>();
        version.Should().NotBeNull();
        version!.VersionNumber.Should().Be("3.0.0");
        version.SoftwareId.Should().Be(softwareId);
        version.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_WithInvalidSoftwareId_ReturnsBadRequest()
    {
        var request = new CreateSoftwareVersionRequest(99999, 0, "1.0.0", null, null);

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("2.5.3-beta")]
    [InlineData("10.20.30.40")]
    [InlineData("2025.1")]
    public async Task Create_WithVariousVersionNumbers_Succeeds(string versionNumber)
    {
        var softwareId = await CreateSoftwareAsync($"Software for {versionNumber}");
        var request = new CreateSoftwareVersionRequest(softwareId, 0, versionNumber, null, null);

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var version = await response.Content.ReadFromJsonAsync<SoftwareVersionResponse>();
        version!.VersionNumber.Should().Be(versionNumber);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsNoContent()
    {
        var softwareId = await CreateSoftwareAsync("Software for Update");
        var createRequest = new CreateSoftwareVersionRequest(softwareId, 0, "1.0.0", "Old details", "Old notes");
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<SoftwareVersionResponse>();

        var updateRequest = new UpdateSoftwareVersionRequest(0, "2.0.0", "New installation", "Updated notes");

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(BaseUrl);
        var versions = await getResponse.Content.ReadFromJsonAsync<List<SoftwareVersionResponse>>();
        versions.Should().Contain(v => v.VersionNumber == "2.0.0");
    }

    [Fact]
    public async Task Update_WhenNotExists_ReturnsNotFound()
    {
        var updateRequest = new UpdateSoftwareVersionRequest(0, "1.0.0", null, null);

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/99999", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        // Arrange - Create a version first
        var softwareId = await CreateSoftwareAsync("Software for Delete");
        var createRequest = new CreateSoftwareVersionRequest(softwareId, 0, "1.0.0", null, null);
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<SoftwareVersionResponse>();

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
