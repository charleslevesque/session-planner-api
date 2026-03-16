using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class LaboratoryService : ILaboratoryService
{
    private readonly AppDbContext _db;

    public LaboratoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Laboratory>> GetAllAsync(string? building, int? minCapacity, int? maxCapacity)
    {
        var query = _db.Laboratories
            .Include(l => l.Workstations)
                .ThenInclude(w => w.OS)
            .AsQueryable();

        if (!string.IsNullOrEmpty(building))
            query = query.Where(l => l.Building == building);

        if (minCapacity.HasValue)
            query = query.Where(l => l.SeatingCapacity >= minCapacity.Value);

        if (maxCapacity.HasValue)
            query = query.Where(l => l.SeatingCapacity <= maxCapacity.Value);

        return await query.ToListAsync();
    }

    public async Task<Laboratory?> GetByIdAsync(int id)
    {
        return await _db.Laboratories
            .Include(l => l.Workstations)
                .ThenInclude(w => w.OS)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Laboratory> CreateAsync(string name, string building, int numberOfPCs, int seatingCapacity)
    {
        var lab = new Laboratory
        {
            Name = name,
            Building = building,
            NumberOfPCs = numberOfPCs,
            SeatingCapacity = seatingCapacity
        };

        _db.Laboratories.Add(lab);
        await _db.SaveChangesAsync();

        return lab;
    }

    public async Task<bool> UpdateAsync(int id, string name, string building, int numberOfPCs, int seatingCapacity)
    {
        var lab = await _db.Laboratories.FindAsync(id);
        if (lab is null)
            return false;

        lab.Name = name;
        lab.Building = building;
        lab.NumberOfPCs = numberOfPCs;
        lab.SeatingCapacity = seatingCapacity;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var lab = await _db.Laboratories.FindAsync(id);
        if (lab is null)
            return false;

        _db.Laboratories.Remove(lab);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<AddWorkstationResult> AddWorkstationAsync(int laboratoryId, string name, int osId)
    {
        var lab = await _db.Laboratories.FindAsync(laboratoryId);
        if (lab is null)
            return new AddWorkstationResult(AddWorkstationStatus.LaboratoryNotFound, null);

        var os = await _db.OperatingSystems.FindAsync(osId);
        if (os is null)
            return new AddWorkstationResult(AddWorkstationStatus.OperatingSystemNotFound, null);

        var workstation = new Workstation
        {
            Name = name,
            LaboratoryId = laboratoryId,
            OSId = osId
        };

        _db.Workstations.Add(workstation);
        await _db.SaveChangesAsync();

        await _db.Entry(workstation).Reference(w => w.OS).LoadAsync();

        return new AddWorkstationResult(AddWorkstationStatus.Success, workstation);
    }

    public async Task<bool> RemoveWorkstationAsync(int laboratoryId, int workstationId)
    {
        var lab = await _db.Laboratories.FindAsync(laboratoryId);
        if (lab is null)
            return false;

        var workstation = await _db.Workstations
            .FirstOrDefaultAsync(w => w.LaboratoryId == laboratoryId && w.Id == workstationId);

        if (workstation is null)
            return false;

        _db.Workstations.Remove(workstation);
        await _db.SaveChangesAsync();

        return true;
    }
}