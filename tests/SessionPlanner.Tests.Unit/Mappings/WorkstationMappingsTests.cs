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
            Name = "Workstation-01",
            LaboratoryId = 1,
            Laboratory = lab,
            OSId = 2,
            OS = os
        };

        var response = workstation.ToResponse();

        response.Id.Should().Be(1);
        response.Name.Should().Be("Workstation-01");
        response.LaboratoryId.Should().Be(1);
        response.OSId.Should().Be(2);
        response.OSName.Should().Be("Windows 11");
    }

    [Theory]
    [InlineData(1, "Windows 11")]
    [InlineData(5, "Ubuntu 22.04")]
    [InlineData(10, "macOS Sonoma")]
    public void ToResponse_ShouldHandleVariousOS(int osId, string osName)
    {
        var os = new OS { Id = osId, Name = osName };
        var workstation = new Workstation
        {
            Id = 1,
            Name = "WS-Test",
            LaboratoryId = 1,
            OSId = osId,
            OS = os
        };

        var response = workstation.ToResponse();

        response.OSId.Should().Be(osId);
        response.OSName.Should().Be(osName);
    }
}
