using SessionPlanner.Api.Dtos.VirtualMachines;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class VirtualMachineMappings
{
    public static VirtualMachineResponse ToResponse(this VirtualMachine vm)
    {
        return new VirtualMachineResponse(
            vm.Id,
            vm.Quantity,
            vm.CpuCores,
            vm.RamGb,
            vm.StorageGb,
            vm.AccessType,
            vm.Notes,
            vm.OSId,
            vm.OS?.Name ?? string.Empty,
            vm.HostServerId,
            vm.HostServer?.Hostname
        );
    }
}
