using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class OperatingSystemService : IOperatingSystemService
{
    private readonly AppDbContext _db;

    public OperatingSystemService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<OS>> GetAllAsync()
    {
        return await _db.OperatingSystems
            .Include(s => s.SoftwareVersions)
            .ToListAsync();
    }

    public async Task<OS?> GetByIdAsync(int id)
    {
        return await _db.OperatingSystems.FindAsync(id);
    }

    public async Task<OS> CreateAsync(string name)
    {
        var os = new OS
        {
            Name = name
        };

        _db.OperatingSystems.Add(os);
        await _db.SaveChangesAsync();

        return os;
    }

    public async Task<bool> UpdateAsync(int id, string name)
    {
        var os = await _db.OperatingSystems.FindAsync(id);
        if (os is null)
            return false;

        os.Name = name;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var os = await _db.OperatingSystems.FindAsync(id);
        if (os is null)
            return false;

        _db.OperatingSystems.Remove(os);
        await _db.SaveChangesAsync();

        return true;
    }
}