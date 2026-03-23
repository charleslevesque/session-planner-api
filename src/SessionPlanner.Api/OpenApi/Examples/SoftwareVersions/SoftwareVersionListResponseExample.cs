using SessionPlanner.Api.Dtos.SoftwareVersions;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.SoftwareVersions;

public sealed class SoftwareVersionListResponseExample : IExamplesProvider<IEnumerable<SoftwareVersionResponse>>
{
    public IEnumerable<SoftwareVersionResponse> GetExamples()
    {
        return
        [
            new SoftwareVersionResponse(
                Id: 1,
                SoftwareId: 1,
                OsId: 2,
                VersionNumber: "2.0.22",
                InstallationDetails: "Install pycharm",
                Notes: "Used in programming labs."
            ),
            new SoftwareVersionResponse(
                Id: 2,
                SoftwareId: 2,
                OsId: 1,
                VersionNumber: "19.1.2",
                InstallationDetails: "Requires Unity",
                Notes: "Used for video game programming classes."
            )
        ];
    }
}