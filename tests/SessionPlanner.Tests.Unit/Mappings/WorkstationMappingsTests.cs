using FluentAssertions;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Tests.Unit.Mappings;

public class WorkstationMappingsTests
{
    [Fact]
    public void ToResponse_ShouldMapAllProperties()
    {
        var os = new OS { Id = 2, Name = "Windows 11" };
        var lab = new Laboratory { Id = 1, Name = "Lab A-3344", Building = "A", NumberOfPCs = 30, SeatingCapacity = 35 };
        var workstation = new Workstation
        {
            Id = 1,
            LaboratoryId = 1,
            Laboratory = lab,
            OperatingSystemId = 2,
            OperatingSystem = os,
            Count = 25
        };

        var response = workstation.ToResponse();

        response.Id.Should().Be(1);
        response.LaboratoryId.Should().Be(1);
        response.OperatingSystemId.Should().Be(2);
        response.OperatingSystemName.Should().Be("Windows 11");
        response.Count.Should().Be(25);
    }

    [Theory]
    [InlineData(1, "Windows 11", 10)]
    [InlineData(5, "Ubuntu 22.04", 25)]
    [InlineData(10, "macOS Sonoma", 50)]
    public void ToResponse_ShouldHandleVariousCounts(int osId, string osName, int count)
    {
        var os = new OS { Id = osId, Name = osName };
        var workstation = new Workstation
        {
            Id = 1,
            LaboratoryId = 1,
            OperatingSystemId = osId,
            OperatingSystem = os,
            Count = count
        };

        var response = workstation.ToResponse();

        response.OperatingSystemId.Should().Be(osId);
        response.OperatingSystemName.Should().Be(osName);
        response.Count.Should().Be(count);
    }
}
