using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class PhysicalServerService : IPhysicalServerService
{
    private readonly AppDbContext _db;

    public PhysicalServerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PhysicalServerOperationResult> CreateAsync(
        string hostname,
        int cpuCores,
        int ramGb,
        int storageGb,
        string accessType,
        string? notes,
        int osId)
    {
        var os = await _db.OperatingSystems.FindAsync(osId);
        if (os is null)
            return new PhysicalServerOperationResult(PhysicalServerOperationStatus.OperatingSystemNotFound, null);

        var existingServer = await _db.PhysicalServers
            .FirstOrDefaultAsync(s => s.Hostname == hostname);
        if (existingServer is not null)
            return new PhysicalServerOperationResult(PhysicalServerOperationStatus.DuplicateHostname, null);

        var server = new PhysicalServer
        {
            Hostname = hostname,
            CpuCores = cpuCores,
            RamGb = ramGb,
            StorageGb = storageGb,
            AccessType = accessType,
            Notes = notes,
            OSId = osId
        };

        _db.PhysicalServers.Add(server);
        await _db.SaveChangesAsync();

        await _db.Entry(server).Reference(s => s.OS).LoadAsync();

        return new PhysicalServerOperationResult(PhysicalServerOperationStatus.Success, server);
    }

    public async Task<List<PhysicalServer>> GetAllAsync()
    {
        return await _db.PhysicalServers
            .Include(s => s.OS)
            .ToListAsync();
    }

    public async Task<PhysicalServer?> GetByIdAsync(int id)
    {
        return await _db.PhysicalServers
            .Include(s => s.OS)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<PhysicalServerOperationStatus> UpdateAsync(
        int id,
        string hostname,
        int cpuCores,
        int ramGb,
        int storageGb,
        string accessType,
        string? notes,
        int osId)
    {
        var server = await _db.PhysicalServers.FindAsync(id);
        if (server is null)
            return PhysicalServerOperationStatus.NotFound;

        var os = await _db.OperatingSystems.FindAsync(osId);
        if (os is null)
            return PhysicalServerOperationStatus.OperatingSystemNotFound;

        var existingServer = await _db.PhysicalServers
            .FirstOrDefaultAsync(s => s.Hostname == hostname && s.Id != id);
        if (existingServer is not null)
            return PhysicalServerOperationStatus.DuplicateHostname;

        server.Hostname = hostname;
        server.CpuCores = cpuCores;
        server.RamGb = ramGb;
        server.StorageGb = storageGb;
        server.AccessType = accessType;
        server.Notes = notes;
        server.OSId = osId;

        await _db.SaveChangesAsync();

        return PhysicalServerOperationStatus.Success;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var server = await _db.PhysicalServers.FindAsync(id);

        if (server is null)
            return false;

        _db.PhysicalServers.Remove(server);
        await _db.SaveChangesAsync();

        return true;
    }
}