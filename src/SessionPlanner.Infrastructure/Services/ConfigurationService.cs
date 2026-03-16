using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly AppDbContext _db;

    public ConfigurationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Configuration> CreateAsync(string title, string? notes)
    {
        var configuration = new Configuration
        {
            Title = title,
            Notes = notes
        };

        _db.Configurations.Add(configuration);
        await _db.SaveChangesAsync();

        return configuration;
    }

    public async Task<List<Configuration>> GetAllAsync()
    {
        return await _db.Configurations.ToListAsync();
    }

    public async Task<Configuration?> GetByIdAsync(int id)
    {
        return await _db.Configurations.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(int id, string title, string? notes)
    {
        var configuration = await _db.Configurations.FindAsync(id);
        if (configuration is null)
            return false;

        configuration.Title = title;
        configuration.Notes = notes;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var configuration = await _db.Configurations.FindAsync(id);
        if (configuration is null)
            return false;

        _db.Configurations.Remove(configuration);
        await _db.SaveChangesAsync();

        return true;
    }
}