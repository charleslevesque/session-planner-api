using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.OperatingSystems;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class OperatingSystemsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/OperatingSystems";

    public OperatingSystemsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region GET Tests

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyList()
    {
        var response = await _client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var osList = await response.Content.ReadFromJsonAsync<List<OSResponse>>();
        osList.Should().NotBeNull();
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
    public async Task Create_WithValidData_ReturnsCreatedOS()
    {
        var request = new CreateOSRequest("Windows 11 Pro");

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var os = await response.Content.ReadFromJsonAsync<OSResponse>();
        os.Should().NotBeNull();
        os!.Name.Should().Be("Windows 11 Pro");
        os.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_ThenGetById_ReturnsCreatedOS()
    {
        var request = new CreateOSRequest("Ubuntu 24.04 LTS");

        var createResponse = await _client.PostAsJsonAsync(BaseUrl, request);
        var created = await createResponse.Content.ReadFromJsonAsync<OSResponse>();

        var getResponse = await _client.GetAsync($"{BaseUrl}/{created!.Id}");
        var retrieved = await getResponse.Content.ReadFromJsonAsync<OSResponse>();

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(created.Id);
        retrieved.Name.Should().Be("Ubuntu 24.04 LTS");
    }

    [Theory]
    [InlineData("Windows 11")]
    [InlineData("macOS Sonoma")]
    [InlineData("Fedora 40")]
    [InlineData("Debian 12")]
    public async Task Create_WithVariousNames_Succeeds(string osName)
    {
        var request = new CreateOSRequest(osName);

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var os = await response.Content.ReadFromJsonAsync<OSResponse>();
        os!.Name.Should().Be(osName);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsNoContent()
    {
        var createRequest = new CreateOSRequest("Old OS Name");
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<OSResponse>();

        var updateRequest = new UpdateOSRequest("Updated OS Name");

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"{BaseUrl}/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<OSResponse>();
        updated!.Name.Should().Be("Updated OS Name");
    }

    [Fact]
    public async Task Update_WhenNotExists_ReturnsNotFound()
    {
        var updateRequest = new UpdateOSRequest("Updated OS Name");

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/999", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        var createRequest = new CreateOSRequest("OS ToDelete");
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<OSResponse>();

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
}
