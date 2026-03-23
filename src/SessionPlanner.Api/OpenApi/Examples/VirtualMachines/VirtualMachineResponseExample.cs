using SessionPlanner.Api.Dtos.VirtualMachines;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.VirtualMachines;

public sealed class VirtualMachineResponseExample : IExamplesProvider<VirtualMachineResponse>
{
    public VirtualMachineResponse GetExamples()
    {
        return new VirtualMachineResponse(
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
        );
    }
}