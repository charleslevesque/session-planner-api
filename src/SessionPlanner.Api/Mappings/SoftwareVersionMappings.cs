using SessionPlanner.Api.Dtos.SoftwareVersions;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class SoftwareVersionMappings
{
    public static SoftwareVersionResponse ToResponse(this SoftwareVersion v)
        => new(v.Id, v.SoftwareId, v.OsId, v.VersionNumber, v.InstallationDetails, v.Notes);

    public static SoftwareVersion toEntity(this CreateSoftwareVersionRequest r)
        => new()
        {
            SoftwareId = r.SoftwareId,
            OsId = r.OsId,
            VersionNumber = r.VersionNumber,
            InstallationDetails =r.InstallationDetails,
            Notes = r.Notes
        };

    public static void Apply(this UpdateSoftwareVersionRequest r, SoftwareVersion v)
    {
        v.OsId = r.OsId;
        v.VersionNumber = r.VersionNumber;
        v.InstallationDetails = r.InstallationDetails;
        v.Notes = r.Notes;
    }
}


