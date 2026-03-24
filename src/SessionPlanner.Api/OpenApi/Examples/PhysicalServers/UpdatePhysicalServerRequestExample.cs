using SessionPlanner.Api.Dtos.PhysicalServers;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.PhysicalServers;

public sealed class UpdatePhysicalServerRequestExample : IExamplesProvider<UpdatePhysicalServerRequest>
{
    public UpdatePhysicalServerRequest GetExamples()
    {
        return new UpdatePhysicalServerRequest(
            Hostname: "examplesrv.etsmtl.ca",
            CpuCores: 24,
            RamGb: 256,
            StorageGb: 8192,
            AccessType: "Individual",
            Notes: "Max: 32GB Ram",
            OSId: 2
        );
    }
}