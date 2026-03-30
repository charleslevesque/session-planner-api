using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SessionPlanner.Api.Dtos.CourseResources;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Tests.Integration.Fixtures;

namespace SessionPlanner.Tests.Integration.Controllers;

public class CourseResourcesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private const string BaseUrl = "/api/v1/Courses";

    public CourseResourcesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<int> SeedCourseWithResources()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var os = new OS { Name = "Ubuntu 24.04" };
        db.OperatingSystems.Add(os);
        await db.SaveChangesAsync();

        var course = new Course { Code = "LOG430", Name = "Architecture logicielle" };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var saas = new SaaSProduct { Name = "SAP S/4HANA", NumberOfAccounts = 30, Notes = "UCC" };
        db.SaaSProducts.Add(saas);

        var software = new Software { Name = "Docker Desktop", InstallCommand = "choco install docker-desktop" };
        db.Softwares.Add(software);

        var config = new Configuration { Title = "Port 8080 ouvert", Notes = "Firewall rule" };
        db.Configurations.Add(config);

        var server = new PhysicalServer
        {
            Hostname = "atreides.logti.etsmtl.ca",
            CpuCores = 32, RamGb = 128, StorageGb = 2000,
            AccessType = "SSH", OSId = os.Id
        };
        db.PhysicalServers.Add(server);
        await db.SaveChangesAsync();

        var vm = new VirtualMachine
        {
            Quantity = 5, CpuCores = 4, RamGb = 8, StorageGb = 100,
            AccessType = "Team", OSId = os.Id, HostServerId = server.Id
        };
        db.VirtualMachines.Add(vm);

        var equip = new EquipmentModel
        {
            Name = "Meta Quest 3", Quantity = 10,
            DefaultAccessories = "Câbles, manettes", Notes = "VR headset"
        };
        db.EquipmentModels.Add(equip);
        await db.SaveChangesAsync();

        db.Set<CourseSaaSProduct>().Add(new CourseSaaSProduct { CourseId = course.Id, SaaSProductId = saas.Id });
        db.Set<CourseSoftware>().Add(new CourseSoftware { CourseId = course.Id, SoftwareId = software.Id });
        db.Set<CourseConfiguration>().Add(new CourseConfiguration { CourseId = course.Id, ConfigurationId = config.Id });
        db.Set<CourseVirtualMachine>().Add(new CourseVirtualMachine { CourseId = course.Id, VirtualMachineId = vm.Id });
        db.Set<CoursePhysicalServer>().Add(new CoursePhysicalServer { CourseId = course.Id, PhysicalServerId = server.Id });
        db.Set<CourseEquipmentModel>().Add(new CourseEquipmentModel { CourseId = course.Id, EquipmentModelId = equip.Id });
        await db.SaveChangesAsync();

        return course.Id;
    }

    private async Task<int> SeedEmptyCourse()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var course = new Course { Code = "LOG210", Name = "Analyse et conception" };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        return course.Id;
    }

    #region Aggregated endpoint

    [Fact]
    public async Task GetResources_ValidCourse_Returns200WithAllTypes()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/resources");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CourseResourcesResponse>();
        body.Should().NotBeNull();
        body!.SaaS.Should().NotBeEmpty();
        body.Softwares.Should().NotBeEmpty();
        body.Configurations.Should().NotBeEmpty();
        body.VirtualMachines.Should().NotBeEmpty();
        body.PhysicalServers.Should().NotBeEmpty();
        body.Equipment.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetResources_InvalidCourse_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/99999/resources");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetResources_CourseWithNoResources_Returns200WithEmptyLists()
    {
        var courseId = await SeedEmptyCourse();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/resources");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CourseResourcesResponse>();
        body.Should().NotBeNull();
        body!.SaaS.Should().BeEmpty();
        body.Softwares.Should().BeEmpty();
        body.Configurations.Should().BeEmpty();
        body.VirtualMachines.Should().BeEmpty();
        body.PhysicalServers.Should().BeEmpty();
        body.Equipment.Should().BeEmpty();
    }

    [Fact]
    public async Task GetResources_ValidCourse_SaaSHasExpectedFields()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/resources");
        var body = await response.Content.ReadFromJsonAsync<CourseResourcesResponse>();

        var saas = body!.SaaS.First();
        saas.Id.Should().BeGreaterThan(0);
        saas.Name.Should().NotBeNullOrWhiteSpace();
        saas.NumberOfAccounts.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetResources_ValidCourse_VmHasOSNameAndHostServer()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/resources");
        var body = await response.Content.ReadFromJsonAsync<CourseResourcesResponse>();

        var vm = body!.VirtualMachines.First();
        vm.Id.Should().BeGreaterThan(0);
        vm.OSName.Should().NotBeNullOrWhiteSpace();
        vm.HostServerHostname.Should().NotBeNullOrWhiteSpace();
        vm.CpuCores.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetResources_ValidCourse_ServerHasOSName()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/resources");
        var body = await response.Content.ReadFromJsonAsync<CourseResourcesResponse>();

        var server = body!.PhysicalServers.First();
        server.Id.Should().BeGreaterThan(0);
        server.Hostname.Should().NotBeNullOrWhiteSpace();
        server.OSName.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Per-type endpoints

    [Fact]
    public async Task GetSaaS_ValidCourse_Returns200()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/saas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<CourseSaaSResponse>>();
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSaaS_InvalidCourse_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/99999/saas");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSoftwares_ValidCourse_Returns200()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/softwares");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<CourseSoftwareResponse>>();
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSoftwares_InvalidCourse_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/99999/softwares");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetConfigurations_ValidCourse_Returns200()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/configurations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<CourseConfigurationResponse>>();
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetConfigurations_InvalidCourse_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/99999/configurations");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVms_ValidCourse_Returns200()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/vms");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<CourseVmResponse>>();
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetVms_InvalidCourse_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/99999/vms");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetServers_ValidCourse_Returns200()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/servers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<CourseServerResponse>>();
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetServers_InvalidCourse_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/99999/servers");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEquipment_ValidCourse_Returns200()
    {
        var courseId = await SeedCourseWithResources();

        var response = await _client.GetAsync($"{BaseUrl}/{courseId}/equipment");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<CourseEquipmentResponse>>();
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetEquipment_InvalidCourse_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/99999/equipment");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region CRUD non-regression

    [Fact]
    public async Task ExistingCrudEndpoints_StillWork()
    {
        var createResponse = await _client.PostAsJsonAsync(BaseUrl, new { Code = "LOG330", Name = "Assurance qualité" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<dynamic>();
        int id = created!.GetProperty("id").GetInt32();

        var getResponse = await _client.GetAsync($"{BaseUrl}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", new { Code = "LOG330", Name = "AQ Updated" });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteResponse = await _client.DeleteAsync($"{BaseUrl}/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDelete = await _client.GetAsync($"{BaseUrl}/{id}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
