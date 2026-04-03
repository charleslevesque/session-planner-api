using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SessionPlanner.Api.Dtos.Configurations;
using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Dtos.OperatingSystems;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class ConfigurationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/v1/Configurations";

    public ConfigurationsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task Create_WithOsAndLaboratory_ReturnsCreatedConfiguration()
    {
        var osId = await CreateOperatingSystemAsync("Windows 11 Config");
        var laboratoryId = await CreateLaboratoryAsync("Lab Config 01");

        var request = new CreateConfigurationRequest(
            Title: "Config cours admin",
            OSIds: [osId],
            LaboratoryIds: [laboratoryId],
            Notes: "Configuration de test"
        );

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ConfigurationResponse>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("Config cours admin");
        created.OSIds.Should().ContainSingle().Which.Should().Be(osId);
        created.LaboratoryIds.Should().ContainSingle().Which.Should().Be(laboratoryId);
        created.Notes.Should().Be("Configuration de test");

        var getResponse = await _client.GetAsync($"{BaseUrl}/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loaded = await getResponse.Content.ReadFromJsonAsync<ConfigurationResponse>();
        loaded.Should().NotBeNull();
        loaded!.OSIds.Should().ContainSingle().Which.Should().Be(osId);
        loaded.LaboratoryIds.Should().ContainSingle().Which.Should().Be(laboratoryId);
    }

    [Fact]
    public async Task Update_WithNewOsAndLaboratory_PersistsChanges()
    {
        var initialOsId = await CreateOperatingSystemAsync("Ubuntu Config 01");
        var initialLaboratoryId = await CreateLaboratoryAsync("Lab Config 02");
        var newOsId = await CreateOperatingSystemAsync("Debian Config 01");
        var newLaboratoryId = await CreateLaboratoryAsync("Lab Config 03");

        var createRequest = new CreateConfigurationRequest(
            Title: "Config initiale",
            OSIds: [initialOsId],
            LaboratoryIds: [initialLaboratoryId],
            Notes: "Notes initiales"
        );

        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ConfigurationResponse>();
        created.Should().NotBeNull();

        var updateRequest = new UpdateConfigurationRequest(
            Title: "Config mise a jour",
            OSIds: [initialOsId, newOsId],
            LaboratoryIds: [initialLaboratoryId, newLaboratoryId],
            Notes: "Notes maj"
        );

        var updateResponse = await _client.PutAsJsonAsync($"{BaseUrl}/{created!.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"{BaseUrl}/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await getResponse.Content.ReadFromJsonAsync<ConfigurationResponse>();
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Config mise a jour");
        updated.OSIds.Should().Contain([initialOsId, newOsId]);
        updated.LaboratoryIds.Should().Contain([initialLaboratoryId, newLaboratoryId]);
        updated.Notes.Should().Be("Notes maj");
    }

    [Fact]
    public async Task Create_WithInvalidOsOrLaboratory_ReturnsBadRequest()
    {
        var request = new CreateConfigurationRequest(
            Title: "Config invalide",
            OSIds: [999999],
            LaboratoryIds: [999999],
            Notes: "Doit echouer"
        );

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<int> CreateOperatingSystemAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/OperatingSystems", new CreateOSRequest(name));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var os = await response.Content.ReadFromJsonAsync<OSResponse>();
        os.Should().NotBeNull();
        return os!.Id;
    }

    private async Task<int> CreateLaboratoryAsync(string name)
    {
        var request = new CreateLaboratoryRequest(name, "Building Test", 20, 25);
        var response = await _client.PostAsJsonAsync("/api/v1/Laboratories", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var lab = await response.Content.ReadFromJsonAsync<LaboratoryResponse>();
        lab.Should().NotBeNull();
        return lab!.Id;
    }
}
