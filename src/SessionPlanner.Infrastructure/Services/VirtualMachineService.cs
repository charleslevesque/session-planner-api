using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class VirtualMachineService : IVirtualMachineService
{
    private readonly AppDbContext _db;

    public VirtualMachineService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<VirtualMachineOperationResult> CreateAsync(
        int quantity,
        int cpuCores,
        int ramGb,
        int storageGb,
        string accessType,
        string? notes,
        int osId,
        int? hostServerId)
    {
        var os = await _db.OperatingSystems.FindAsync(osId);
        if (os is null)
            return new VirtualMachineOperationResult(VirtualMachineOperationStatus.OperatingSystemNotFound, null);

        if (hostServerId.HasValue)
        {
            var hostServer = await _db.PhysicalServers.FindAsync(hostServerId.Value);
            if (hostServer is null)
                return new VirtualMachineOperationResult(VirtualMachineOperationStatus.HostServerNotFound, null);
        }

        var vm = new VirtualMachine
        {
            Quantity = quantity,
            CpuCores = cpuCores,
            RamGb = ramGb,
            StorageGb = storageGb,
            AccessType = accessType,
            Notes = notes,
            OSId = osId,
            HostServerId = hostServerId
        };

        _db.VirtualMachines.Add(vm);
        await _db.SaveChangesAsync();

        await _db.Entry(vm).Reference(v => v.OS).LoadAsync();
        if (vm.HostServerId.HasValue)
            await _db.Entry(vm).Reference(v => v.HostServer).LoadAsync();

        return new VirtualMachineOperationResult(VirtualMachineOperationStatus.Success, vm);
    }

    public async Task<List<VirtualMachine>> GetAllAsync()
    {
        return await _db.VirtualMachines
            .Include(v => v.OS)
            .Include(v => v.HostServer)
            .ToListAsync();
    }

    public async Task<VirtualMachine?> GetByIdAsync(int id)
    {
        return await _db.VirtualMachines
            .Include(v => v.OS)
            .Include(v => v.HostServer)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<VirtualMachineOperationStatus> UpdateAsync(
        int id,
        int quantity,
        int cpuCores,
        int ramGb,
        int storageGb,
        string accessType,
        string? notes,
        int osId,
        int? hostServerId)
    {
        var vm = await _db.VirtualMachines.FindAsync(id);
        if (vm is null)
            return VirtualMachineOperationStatus.NotFound;

        var os = await _db.OperatingSystems.FindAsync(osId);
        if (os is null)
            return VirtualMachineOperationStatus.OperatingSystemNotFound;

        if (hostServerId.HasValue)
        {
            var hostServer = await _db.PhysicalServers.FindAsync(hostServerId.Value);
            if (hostServer is null)
                return VirtualMachineOperationStatus.HostServerNotFound;
        }

        vm.Quantity = quantity;
        vm.CpuCores = cpuCores;
        vm.RamGb = ramGb;
        vm.StorageGb = storageGb;
        vm.AccessType = accessType;
        vm.Notes = notes;
        vm.OSId = osId;
        vm.HostServerId = hostServerId;

        await _db.SaveChangesAsync();
        return VirtualMachineOperationStatus.Success;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vm = await _db.VirtualMachines.FindAsync(id);

        if (vm is null)
            return false;

        _db.VirtualMachines.Remove(vm);
        await _db.SaveChangesAsync();

        return true;
    }
}