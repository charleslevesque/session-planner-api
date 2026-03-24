using SessionPlanner.Api.Dtos.VirtualMachines;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.VirtualMachines;

public sealed class UpdateVirtualMachineRequestExample : IExamplesProvider<UpdateVirtualMachineRequest>
{
    public UpdateVirtualMachineRequest GetExamples()
    {
        return new UpdateVirtualMachineRequest(
            Quantity: 15,
            CpuCores: 6,
            RamGb: 24,
            StorageGb: 300,
            AccessType: "Individual",
            Notes: "Expanded VM capacity",
            OSId: 2,
            HostServerId: 1
        );
    }
}