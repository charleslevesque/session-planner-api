using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class LaboratorySoftwareService : ILaboratorySoftwareService
{
    private readonly AppDbContext _db;

    public LaboratorySoftwareService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LaboratorySoftware>> GetAllAsync()
    {
        return await _db.LaboratorySoftwares
            .Include(ls => ls.Laboratory)
            .Include(ls => ls.Software)
            .ToListAsync();
    }

    public async Task<List<LaboratorySoftware>> GetByLaboratoryAsync(int laboratoryId)
    {
        return await _db.LaboratorySoftwares
            .Include(ls => ls.Laboratory)
            .Include(ls => ls.Software)
            .Where(ls => ls.LaboratoryId == laboratoryId)
            .ToListAsync();
    }

    public async Task<LaboratorySoftware?> UpsertAsync(int laboratoryId, int softwareId, string status)
    {
        var lab = await _db.Laboratories.FindAsync(laboratoryId);
        if (lab is null) return null;

        var software = await _db.Softwares.FindAsync(softwareId);
        if (software is null) return null;

        var existing = await _db.LaboratorySoftwares
            .FirstOrDefaultAsync(ls => ls.LaboratoryId == laboratoryId && ls.SoftwareId == softwareId);

        if (existing is not null)
        {
            existing.Status = status;
        }
        else
        {
            existing = new LaboratorySoftware
            {
                LaboratoryId = laboratoryId,
                SoftwareId = softwareId,
                Status = status
            };
            _db.LaboratorySoftwares.Add(existing);
        }

        await _db.SaveChangesAsync();

        await _db.Entry(existing).Reference(ls => ls.Laboratory).LoadAsync();
        await _db.Entry(existing).Reference(ls => ls.Software).LoadAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(int laboratoryId, int softwareId)
    {
        var entry = await _db.LaboratorySoftwares
            .FirstOrDefaultAsync(ls => ls.LaboratoryId == laboratoryId && ls.SoftwareId == softwareId);

        if (entry is null) return false;

        _db.LaboratorySoftwares.Remove(entry);
        await _db.SaveChangesAsync();
        return true;
    }
}
