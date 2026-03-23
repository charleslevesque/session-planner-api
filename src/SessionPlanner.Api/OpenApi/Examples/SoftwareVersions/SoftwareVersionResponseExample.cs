using SessionPlanner.Api.Dtos.SoftwareVersions;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.SoftwareVersions;

public sealed class SoftwareVersionResponseExample : IExamplesProvider<SoftwareVersionResponse>
{
    public SoftwareVersionResponse GetExamples()
    {
        return new SoftwareVersionResponse(
            Id: 1,
            SoftwareId: 1,
            OsId: 2,
            VersionNumber: "2.0.22",
            InstallationDetails: "Install pycharm",
            Notes: "Used in programming labs."
        );
    }
}