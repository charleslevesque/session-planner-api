using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class SoftwareService : ISoftwareService
{
    private readonly AppDbContext _db;

    public SoftwareService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Software> CreateAsync(string name)
    {
        var software = new Software
        {
            Name = name
        };

        _db.Softwares.Add(software);
        await _db.SaveChangesAsync();

        return software;
    }

    public async Task<List<Software>> GetAllAsync()
    {
        return await _db.Softwares
            .Include(s => s.SoftwareVersions)
            .ToListAsync();
    }

    public async Task<Software?> GetByIdAsync(int id)
    {
        return await _db.Softwares
            .Include(s => s.SoftwareVersions)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> UpdateAsync(int id, string name)
    {
        var software = await _db.Softwares.FindAsync(id);
        if (software is null)
            return false;

        software.Name = name;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var software = await _db.Softwares.FindAsync(id);
        if (software is null)
            return false;

        _db.Softwares.Remove(software);
        await _db.SaveChangesAsync();

        return true;
    }
}