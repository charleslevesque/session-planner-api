using FluentAssertions;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Tests.Unit.Mappings;

public class SoftwareMappingsTests
{
    [Fact]
    public void ToResponse_ShouldMapAllProperties()
    {
        var software = new Software
        {
            Id = 1,
            Name = "Visual Studio Code"
        };

        var response = software.ToResponse();

        response.Id.Should().Be(1);
        response.Name.Should().Be("Visual Studio Code");
    }

    [Theory]
    [InlineData("Visual Studio Code")]
    [InlineData("JetBrains IntelliJ IDEA")]
    [InlineData("Adobe Photoshop")]
    [InlineData("Microsoft Office 365")]
    public void ToResponse_ShouldHandleVariousSoftwareNames(string softwareName)
    {
        var software = new Software { Id = 1, Name = softwareName };

        var response = software.ToResponse();

        response.Name.Should().Be(softwareName);
    }
}
