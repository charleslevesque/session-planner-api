using FluentAssertions;
using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Dtos.Workstations;

namespace SessionPlanner.Tests.Unit.Dtos;

public class WorkstationDtosTests
{
    [Fact]
    public void AddWorkstationRequest_ShouldStoreAllProperties()
    {
        var request = new AddWorkstationRequest(1, 25);

        request.OperatingSystemId.Should().Be(1);
        request.Count.Should().Be(25);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(5, 50)]
    [InlineData(10, 100)]
    public void AddWorkstationRequest_ShouldAcceptVariousValues(int osId, int count)
    {
        var request = new AddWorkstationRequest(osId, count);

        request.OperatingSystemId.Should().Be(osId);
        request.Count.Should().Be(count);
    }

    [Fact]
    public void WorkstationResponse_ShouldStoreAllProperties()
    {
        var response = new WorkstationResponse(1, 2, 3, "Windows 11", 30);

        response.Id.Should().Be(1);
        response.LaboratoryId.Should().Be(2);
        response.OperatingSystemId.Should().Be(3);
        response.OperatingSystemName.Should().Be("Windows 11");
        response.Count.Should().Be(30);
    }

    [Theory]
    [InlineData(1, 1, 1, "Windows 11", 10)]
    [InlineData(10, 5, 3, "Ubuntu 24.04", 25)]
    [InlineData(100, 50, 10, "macOS Sonoma", 50)]
    public void WorkstationResponse_ShouldAcceptVariousValues(int id, int labId, int osId, string osName, int count)
    {
        var response = new WorkstationResponse(id, labId, osId, osName, count);

        response.Id.Should().Be(id);
        response.LaboratoryId.Should().Be(labId);
        response.OperatingSystemId.Should().Be(osId);
        response.OperatingSystemName.Should().Be(osName);
        response.Count.Should().Be(count);
    }
}
