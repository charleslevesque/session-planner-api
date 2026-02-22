using FluentAssertions;
using SessionPlanner.Api.Dtos.Softwares;

namespace SessionPlanner.Tests.Unit.Dtos;

public class SoftwareDtosTests
{
    [Fact]
    public void CreateSoftwareRequest_ShouldStoreNameProperty()
    {
        var request = new CreateSoftwareRequest("Visual Studio Code");

        request.Name.Should().Be("Visual Studio Code");
    }

    [Theory]
    [InlineData("Adobe Photoshop")]
    [InlineData("Microsoft Office")]
    [InlineData("JetBrains IntelliJ")]
    public void CreateSoftwareRequest_ShouldAcceptVariousNames(string name)
    {
        var request = new CreateSoftwareRequest(name);

        request.Name.Should().Be(name);
    }

    [Fact]
    public void UpdateSoftwareRequest_ShouldStoreNameProperty()
    {
        var request = new UpdateSoftwareRequest("Updated Name");

        request.Name.Should().Be("Updated Name");
    }

    [Theory]
    [InlineData("New Software Name")]
    [InlineData("Another Update")]
    [InlineData("")]
    public void UpdateSoftwareRequest_ShouldAcceptVariousNames(string name)
    {
        var request = new UpdateSoftwareRequest(name);

        request.Name.Should().Be(name);
    }

    [Fact]
    public void SoftwareResponse_ShouldStoreAllProperties()
    {
        var response = new SoftwareResponse(1, "Test Software");

        response.Id.Should().Be(1);
        response.Name.Should().Be("Test Software");
    }

    [Theory]
    [InlineData(1, "Software A")]
    [InlineData(100, "Software B")]
    [InlineData(999, "Software with long name here")]
    public void SoftwareResponse_ShouldAcceptVariousValues(int id, string name)
    {
        var response = new SoftwareResponse(id, name);

        response.Id.Should().Be(id);
        response.Name.Should().Be(name);
    }
}
