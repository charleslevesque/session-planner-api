using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
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

    public async Task<Configuration> CreateAsync(string title, IReadOnlyList<int> osIds, IReadOnlyList<int> laboratoryIds, string? notes)
    {
        var configuration = new Configuration
        {
            Title = title,
            Notes = notes
        };

        _db.Configurations.Add(configuration);
        await _db.SaveChangesAsync();

        await SetOsAssociationsAsync(configuration.Id, osIds);
        await SetLaboratoryAssociationsAsync(configuration.Id, laboratoryIds);
        await _db.SaveChangesAsync();

        return await GetConfigurationWithAssociationsAsync(configuration.Id)
            ?? configuration;
    }

    public async Task<List<Configuration>> GetAllAsync()
    {
        return await _db.Configurations
            .AsNoTracking()
            .Include(c => c.ConfigurationOSes)
            .Include(c => c.LaboratoryConfigurations)
            .ToListAsync();
    }

    public async Task<Configuration?> GetByIdAsync(int id)
    {
        return await _db.Configurations
            .AsNoTracking()
            .Include(c => c.ConfigurationOSes)
            .Include(c => c.LaboratoryConfigurations)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> UpdateAsync(int id, string title, IReadOnlyList<int> osIds, IReadOnlyList<int> laboratoryIds, string? notes)
    {
        var configuration = await _db.Configurations.FindAsync(id);
        if (configuration is null)
            return false;

        configuration.Title = title;
        configuration.Notes = notes;

        await SetOsAssociationsAsync(configuration.Id, osIds);
        await SetLaboratoryAssociationsAsync(configuration.Id, laboratoryIds);

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

    private async Task<Configuration?> GetConfigurationWithAssociationsAsync(int configurationId)
    {
        return await _db.Configurations
            .AsNoTracking()
            .Include(c => c.ConfigurationOSes)
            .Include(c => c.LaboratoryConfigurations)
            .FirstOrDefaultAsync(c => c.Id == configurationId);
    }

    private async Task SetOsAssociationsAsync(int configurationId, IReadOnlyList<int> osIds)
    {
        var requestedIds = osIds.Distinct().ToHashSet();
        var existing = await _db.Set<ConfigurationOS>()
            .Where(x => x.ConfigurationId == configurationId)
            .ToListAsync();

        if (requestedIds.Count == 0)
        {
            if (existing.Count > 0)
                _db.Set<ConfigurationOS>().RemoveRange(existing);
            return;
        }

        var existingOsIds = await _db.OperatingSystems
            .Where(x => requestedIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        if (existingOsIds.Count != requestedIds.Count)
            throw new ArgumentException("Un ou plusieurs OS sont introuvables.");

        var toAdd = requestedIds
            .Except(existing.Select(x => x.OSId))
            .Select(osId => new ConfigurationOS
            {
                ConfigurationId = configurationId,
                OSId = osId,
            })
            .ToList();

        if (toAdd.Count > 0)
            _db.Set<ConfigurationOS>().AddRange(toAdd);

        var stale = existing.Where(x => !requestedIds.Contains(x.OSId)).ToList();
        if (stale.Count > 0)
            _db.Set<ConfigurationOS>().RemoveRange(stale);
    }

    private async Task SetLaboratoryAssociationsAsync(int configurationId, IReadOnlyList<int> laboratoryIds)
    {
        var requestedIds = laboratoryIds.Distinct().ToHashSet();
        var existing = await _db.Set<LaboratoryConfiguration>()
            .Where(x => x.ConfigurationId == configurationId)
            .ToListAsync();

        if (requestedIds.Count == 0)
        {
            if (existing.Count > 0)
                _db.Set<LaboratoryConfiguration>().RemoveRange(existing);
            return;
        }

        var existingLaboratoryIds = await _db.Laboratories
            .Where(x => requestedIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        if (existingLaboratoryIds.Count != requestedIds.Count)
            throw new ArgumentException("Un ou plusieurs laboratoires sont introuvables.");

        var toAdd = requestedIds
            .Except(existing.Select(x => x.LaboratoryId))
            .Select(laboratoryId => new LaboratoryConfiguration
            {
                ConfigurationId = configurationId,
                LaboratoryId = laboratoryId,
            })
            .ToList();

        if (toAdd.Count > 0)
            _db.Set<LaboratoryConfiguration>().AddRange(toAdd);

        var stale = existing.Where(x => !requestedIds.Contains(x.LaboratoryId)).ToList();
        if (stale.Count > 0)
            _db.Set<LaboratoryConfiguration>().RemoveRange(stale);
    }
}