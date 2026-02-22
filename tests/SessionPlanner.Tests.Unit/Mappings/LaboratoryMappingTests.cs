using FluentAssertions;
using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Tests.Unit.Mappings;

public class LaboratoryMappingTests
{
    [Fact]
    public void ToResponse_ShouldMapAllProperties()
    {
        var os = new OS { Id = 1, Name = "Windows 11" };
        var lab = new Laboratory
        {
            Id = 1,
            Name = "Lab A-3344",
            Building = "Building A",
            NumberOfPCs = 30,
            SeatingCapacity = 35,
            Workstations = new List<Workstation>
            {
                new() { Id = 1, LaboratoryId = 1, OperatingSystemId = 1, OperatingSystem = os, Count = 25 }
            }
        };

        var response = lab.ToResponse();

        response.Id.Should().Be(1);
        response.Name.Should().Be("Lab A-3344");
        response.Building.Should().Be("Building A");
        response.NumberOfPCs.Should().Be(30);
        response.SeatingCapacity.Should().Be(35);
        response.Workstations.Should().HaveCount(1);
        response.Workstations.First().OperatingSystemName.Should().Be("Windows 11");
    }

    [Fact]
    public void ToResponse_WithEmptyWorkstations_ShouldReturnEmptyList()
    {
        var lab = new Laboratory
        {
            Id = 2,
            Name = "Lab B-200",
            Building = "Building B",
            NumberOfPCs = 20,
            SeatingCapacity = 25,
            Workstations = new List<Workstation>()
        };

        var response = lab.ToResponse();

        response.Workstations.Should().BeEmpty();
    }

    [Fact]
    public void ToEntity_ShouldMapFromCreateRequest()
    {
        var request = new CreateLaboratoryRequest("Lab C-300", "Building C", 40, 45);

        var entity = request.ToEntity();

        entity.Name.Should().Be("Lab C-300");
        entity.Building.Should().Be("Building C");
        entity.NumberOfPCs.Should().Be(40);
        entity.SeatingCapacity.Should().Be(45);
        entity.Id.Should().Be(0); // Not set yet
    }

    [Fact]
    public void Apply_ShouldUpdateAllProperties()
    {
        var lab = new Laboratory
        {
            Id = 1,
            Name = "Old Name",
            Building = "Old Building",
            NumberOfPCs = 10,
            SeatingCapacity = 15
        };
        var updateRequest = new UpdateLaboratoryRequest("New Name", "New Building", 50, 55);

        updateRequest.Apply(lab);

        lab.Id.Should().Be(1); // Should not change
        lab.Name.Should().Be("New Name");
        lab.Building.Should().Be("New Building");
        lab.NumberOfPCs.Should().Be(50);
        lab.SeatingCapacity.Should().Be(55);
    }
}
