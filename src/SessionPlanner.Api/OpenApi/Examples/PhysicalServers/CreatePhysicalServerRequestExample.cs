using SessionPlanner.Api.Dtos.PhysicalServers;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.PhysicalServers;

public sealed class CreatePhysicalServerRequestExample : IExamplesProvider<CreatePhysicalServerRequest>
{
    public CreatePhysicalServerRequest GetExamples()
    {
        return new CreatePhysicalServerRequest(
            Hostname: "examplesrv.example.ca",
            CpuCores: 24,
            RamGb: 256,
            StorageGb: 4916,
            AccessType: "Individual",
            Notes: "Max: 16GB Ram",
            OSId: 1
        );
    }
}