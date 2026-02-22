using FluentAssertions;
using SessionPlanner.Api.Dtos.OperatingSystems;

namespace SessionPlanner.Tests.Unit.Dtos;

public class OperatingSystemDtosTests
{
    [Fact]
    public void CreateOSRequest_ShouldStoreNameProperty()
    {
        var request = new CreateOSRequest("Windows 11");

        request.Name.Should().Be("Windows 11");
    }

    [Theory]
    [InlineData("Ubuntu 24.04")]
    [InlineData("macOS Sonoma")]
    [InlineData("Fedora 40")]
    public void CreateOSRequest_ShouldAcceptVariousNames(string name)
    {
        var request = new CreateOSRequest(name);

        request.Name.Should().Be(name);
    }

    [Fact]
    public void UpdateOSRequest_ShouldStoreNameProperty()
    {
        var request = new UpdateOSRequest("Windows 11 Pro");

        request.Name.Should().Be("Windows 11 Pro");
    }

    [Fact]
    public void OSResponse_ShouldStoreAllProperties()
    {
        var response = new OSResponse(1, "Debian 12");

        response.Id.Should().Be(1);
        response.Name.Should().Be("Debian 12");
    }

    [Theory]
    [InlineData(1, "Windows 11")]
    [InlineData(50, "Linux Mint")]
    [InlineData(999, "Chrome OS")]
    public void OSResponse_ShouldAcceptVariousValues(int id, string name)
    {
        var response = new OSResponse(id, name);

        response.Id.Should().Be(id);
        response.Name.Should().Be(name);
    }
}
