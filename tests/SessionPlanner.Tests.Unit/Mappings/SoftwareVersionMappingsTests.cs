using FluentAssertions;
using SessionPlanner.Api.Dtos.SoftwareVersions;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Tests.Unit.Mappings;

public class SoftwareVersionMappingsTests
{
    [Fact]
    public void ToResponse_ShouldMapAllProperties()
    {
        var version = new SoftwareVersion
        {
            Id = 1,
            SoftwareId = 10,
            OsId = 2,
            VersionNumber = "1.85.0",
            InstallationDetails = "Download from website",
            Notes = "Requires .NET 8"
        };

        var response = version.ToResponse();

        response.Id.Should().Be(1);
        response.SoftwareId.Should().Be(10);
        response.OsId.Should().Be(2);
        response.VersionNumber.Should().Be("1.85.0");
        response.InstallationDetails.Should().Be("Download from website");
        response.Notes.Should().Be("Requires .NET 8");
    }

    [Fact]
    public void ToResponse_WithNullOptionalFields_ShouldReturnNulls()
    {
        var version = new SoftwareVersion
        {
            Id = 1,
            SoftwareId = 10,
            OsId = 2,
            VersionNumber = "2.0.0",
            InstallationDetails = null,
            Notes = null
        };

        var response = version.ToResponse();

        response.InstallationDetails.Should().BeNull();
        response.Notes.Should().BeNull();
    }

    [Fact]
    public void ToEntity_ShouldMapFromCreateRequest()
    {
        var request = new CreateSoftwareVersionRequest(10, 2, "1.0.0", "Install via MSI", "First release");

        var entity = request.toEntity();

        entity.SoftwareId.Should().Be(10);
        entity.OsId.Should().Be(2);
        entity.VersionNumber.Should().Be("1.0.0");
        entity.InstallationDetails.Should().Be("Install via MSI");
        entity.Notes.Should().Be("First release");
        entity.Id.Should().Be(0); // Not set yet
    }

    [Fact]
    public void Apply_ShouldUpdateAllProperties()
    {
        var version = new SoftwareVersion
        {
            Id = 1,
            SoftwareId = 10,
            OsId = 2,
            VersionNumber = "1.0.0",
            InstallationDetails = "Old details",
            Notes = "Old notes"
        };
        var updateRequest = new UpdateSoftwareVersionRequest(3, "2.0.0", "New installation", "Updated notes");

        updateRequest.Apply(version);

        version.Id.Should().Be(1); // Should not change
        version.SoftwareId.Should().Be(10); // Should not change 
        version.OsId.Should().Be(3);
        version.VersionNumber.Should().Be("2.0.0");
        version.InstallationDetails.Should().Be("New installation");
        version.Notes.Should().Be("Updated notes");
    }
}
