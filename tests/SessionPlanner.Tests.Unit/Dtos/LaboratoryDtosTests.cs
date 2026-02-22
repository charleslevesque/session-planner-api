using FluentAssertions;
using SessionPlanner.Api.Dtos.Laboratories;

namespace SessionPlanner.Tests.Unit.Dtos;

public class LaboratoryDtosTests
{
    [Fact]
    public void CreateLaboratoryRequest_ShouldStoreAllProperties()
    {
        var request = new CreateLaboratoryRequest("Lab A", "Building A", 30, 35);

        request.Name.Should().Be("Lab A");
        request.Building.Should().Be("Building A");
        request.NumberOfPCs.Should().Be(30);
        request.SeatingCapacity.Should().Be(35);
    }

    [Fact]
    public void UpdateLaboratoryRequest_ShouldStoreAllProperties()
    {
        var request = new UpdateLaboratoryRequest("Lab B", "Building B", 40, 45);

        request.Name.Should().Be("Lab B");
        request.Building.Should().Be("Building B");
        request.NumberOfPCs.Should().Be(40);
        request.SeatingCapacity.Should().Be(45);
    }

    [Fact]
    public void LaboratoryResponse_ShouldStoreAllProperties()
    {
        var workstations = new List<SessionPlanner.Api.Dtos.Workstations.WorkstationResponse>();
        var response = new LaboratoryResponse(1, "Lab C", "Building C", 50, 55, workstations);

        response.Id.Should().Be(1);
        response.Name.Should().Be("Lab C");
        response.Building.Should().Be("Building C");
        response.NumberOfPCs.Should().Be(50);
        response.SeatingCapacity.Should().Be(55);
        response.Workstations.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 15)]
    [InlineData(100, 120)]
    public void LaboratoryRequest_ShouldAcceptVariousCapacities(int pcs, int seats)
    {
        var request = new CreateLaboratoryRequest("Test Lab", "Test Building", pcs, seats);

        request.NumberOfPCs.Should().Be(pcs);
        request.SeatingCapacity.Should().Be(seats);
    }
}
