using SessionPlanner.Api.Dtos.SoftwareVersions;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.SoftwareVersions;

public sealed class CreateSoftwareVersionRequestExample : IExamplesProvider<CreateSoftwareVersionRequest>
{
    public CreateSoftwareVersionRequest GetExamples()
    {
        return new CreateSoftwareVersionRequest(
            SoftwareId: 1,
            OsId: 2,
            VersionNumber: "2.0.22",
            InstallationDetails: "Install pycharm",
            Notes: "Used in programming labs."
        );
    }
}