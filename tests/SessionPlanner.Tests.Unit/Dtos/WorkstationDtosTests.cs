using FluentAssertions;
using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Dtos.Workstations;

namespace SessionPlanner.Tests.Unit.Dtos;

public class WorkstationDtosTests
{
    [Fact]
    public void AddWorkstationRequest_ShouldStoreAllProperties()
    {
        var request = new AddWorkstationRequest("Workstation-01", 1);

        request.Name.Should().Be("Workstation-01");
        request.OSId.Should().Be(1);
    }

    [Theory]
    [InlineData("WS-1", 1)]
    [InlineData("WS-5", 5)]
    [InlineData("WS-10", 10)]
    public void AddWorkstationRequest_ShouldAcceptVariousValues(string name, int osId)
    {
        var request = new AddWorkstationRequest(name, osId);

        request.Name.Should().Be(name);
        request.OSId.Should().Be(osId);
    }

    [Fact]
    public void WorkstationResponse_ShouldStoreAllProperties()
    {
        var response = new WorkstationResponse(1, "Workstation-01", 2, 3, "Windows 11");

        response.Id.Should().Be(1);
        response.Name.Should().Be("Workstation-01");
        response.LaboratoryId.Should().Be(2);
        response.OSId.Should().Be(3);
        response.OSName.Should().Be("Windows 11");
    }

    [Theory]
    [InlineData(1, "WS-1", 1, 1, "Windows 11")]
    [InlineData(10, "WS-5", 5, 3, "Ubuntu 24.04")]
    [InlineData(100, "WS-50", 50, 10, "macOS Sonoma")]
    public void WorkstationResponse_ShouldAcceptVariousValues(int id, string name, int labId, int osId, string osName)
    {
        var response = new WorkstationResponse(id, name, labId, osId, osName);

        response.Id.Should().Be(id);
        response.Name.Should().Be(name);
        response.LaboratoryId.Should().Be(labId);
        response.OSId.Should().Be(osId);
        response.OSName.Should().Be(osName);
    }
}
