using SessionPlanner.Api.Dtos.PhysicalServers;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.PhysicalServers;

public sealed class PhysicalServerListResponseExample : IExamplesProvider<IEnumerable<PhysicalServerResponse>>
{
    public IEnumerable<PhysicalServerResponse> GetExamples()
    {
        return
        [
            new PhysicalServerResponse(
                Id: 1,
                Hostname: "examplesrv.example.ca",
                CpuCores: 24,
                RamGb: 256,
                StorageGb: 4916,
                AccessType: "Individual",
                Notes: "Max: 16GB Ram",
                OSId: 1,
                OSName: "Windows"
            ),
            new PhysicalServerResponse(
                Id: 2,
                Hostname: "examplesrv2.example.ca",
                CpuCores: 16,
                RamGb: 128,
                StorageGb: 4096,
                AccessType: "Individual",
                Notes: "Max: 32GB Ram",
                OSId: 2,
                OSName: "Linux"
            )
        ];
    }
}