using FluentAssertions;
using SessionPlanner.Api.Dtos.SoftwareVersions;

namespace SessionPlanner.Tests.Unit.Dtos;

public class SoftwareVersionDtosTests
{
    [Fact]
    public void CreateSoftwareVersionRequest_ShouldStoreAllProperties()
    {
        var request = new CreateSoftwareVersionRequest(1, 2, "1.0.0", "Install via MSI", "First release");

        request.SoftwareId.Should().Be(1);
        request.OsId.Should().Be(2);
        request.VersionNumber.Should().Be("1.0.0");
        request.InstallationDetails.Should().Be("Install via MSI");
        request.Notes.Should().Be("First release");
    }

    [Fact]
    public void CreateSoftwareVersionRequest_WithNullOptionals_ShouldWork()
    {
        var request = new CreateSoftwareVersionRequest(1, 2, "2.0.0", null, null);

        request.InstallationDetails.Should().BeNull();
        request.Notes.Should().BeNull();
    }

    [Fact]
    public void UpdateSoftwareVersionRequest_ShouldStoreAllProperties()
    {
        var request = new UpdateSoftwareVersionRequest(3, "3.0.0", "New installation", "Updated notes");

        request.OsId.Should().Be(3);
        request.VersionNumber.Should().Be("3.0.0");
        request.InstallationDetails.Should().Be("New installation");
        request.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public void SoftwareVersionResponse_ShouldStoreAllProperties()
    {
        var response = new SoftwareVersionResponse(1, 10, 2, "1.5.0", "Download from website", "Requires admin");

        response.Id.Should().Be(1);
        response.SoftwareId.Should().Be(10);
        response.OsId.Should().Be(2);
        response.VersionNumber.Should().Be("1.5.0");
        response.InstallationDetails.Should().Be("Download from website");
        response.Notes.Should().Be("Requires admin");
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("2.5.3-beta")]
    [InlineData("10.20.30")]
    [InlineData("2025.1.0")]
    public void VersionNumber_ShouldAcceptVariousFormats(string version)
    {
        var request = new CreateSoftwareVersionRequest(1, 1, version, null, null);

        request.VersionNumber.Should().Be(version);
    }
}
