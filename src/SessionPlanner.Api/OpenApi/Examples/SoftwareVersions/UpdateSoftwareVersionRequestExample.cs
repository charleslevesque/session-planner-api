using SessionPlanner.Api.Dtos.SoftwareVersions;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.SoftwareVersions;

public sealed class UpdateSoftwareVersionRequestExample : IExamplesProvider<UpdateSoftwareVersionRequest>
{
    public UpdateSoftwareVersionRequest GetExamples()
    {
        return new UpdateSoftwareVersionRequest(
            OsId: 1,
            VersionNumber: "3.0.0",
            InstallationDetails: "Install pycharm",
            Notes: "Used in programming labs."
        );
    }
}