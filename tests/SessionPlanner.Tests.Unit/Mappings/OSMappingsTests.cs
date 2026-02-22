using FluentAssertions;
using SessionPlanner.Api.Dtos.OperatingSystems;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Tests.Unit.Mappings;

public class OSMappingsTests
{
    [Fact]
    public void ToResponse_ShouldMapAllProperties()
    {
        var os = new OS
        {
            Id = 1,
            Name = "Windows 11"
        };

        var response = os.ToResponse();

        response.Id.Should().Be(1);
        response.Name.Should().Be("Windows 11");
    }

    [Fact]
    public void ToEntity_ShouldMapFromCreateRequest()
    {
        var request = new CreateOSRequest("Ubuntu 24.04");

        var entity = request.toEntity();

        entity.Name.Should().Be("Ubuntu 24.04");
        entity.Id.Should().Be(0); // Not set yet
    }

    [Fact]
    public void Apply_ShouldUpdateName()
    {
        var os = new OS
        {
            Id = 1,
            Name = "Old Name"
        };
        var updateRequest = new UpdateOSRequest("macOS Ventura");

        updateRequest.Apply(os);

        os.Id.Should().Be(1); // Should not change
        os.Name.Should().Be("macOS Ventura");
    }

    [Theory]
    [InlineData("Windows 11")]
    [InlineData("Ubuntu 22.04 LTS")]
    [InlineData("macOS Sonoma")]
    [InlineData("Fedora 40")]
    public void ToResponse_ShouldHandleVariousOSNames(string osName)
    {
        var os = new OS { Id = 1, Name = osName };

        var response = os.ToResponse();

        response.Name.Should().Be(osName);
    }
}
