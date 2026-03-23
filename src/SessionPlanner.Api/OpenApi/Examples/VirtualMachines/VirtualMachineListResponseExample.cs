using SessionPlanner.Api.Dtos.VirtualMachines;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.VirtualMachines;

public sealed class VirtualMachineListResponseExample : IExamplesProvider<IEnumerable<VirtualMachineResponse>>
{
    public IEnumerable<VirtualMachineResponse> GetExamples()
    {
        return
        [
            new VirtualMachineResponse(
                Id: 1,
                Quantity: 12,
                CpuCores: 4,
                RamGb: 16,
                StorageGb: 200,
                AccessType: "Individual",
                Notes: "Indiviual access.",
                OSId: 1,
                OSName: "Windows",
                HostServerId: 1,
                HostServerHostname: "srv-ex-01"
            ),
            new VirtualMachineResponse(
                Id: 2,
                Quantity: 34,
                CpuCores: 8,
                RamGb: 16,
                StorageGb: 2048,
                AccessType: "Team",
                Notes: "Team access.",
                OSId: 2,
                OSName: "Linux",
                HostServerId: 3,
                HostServerHostname: "srv-ex-03"
            )
        ];
    }
}