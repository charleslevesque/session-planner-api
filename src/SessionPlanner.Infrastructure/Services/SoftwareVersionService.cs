using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class SoftwareVersionService : ISoftwareVersionService
{
    private readonly AppDbContext _db;

    public SoftwareVersionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SoftwareVersion?> CreateAsync(int softwareId, int osId, string versionNumber, string? installationDetails, string? notes)
    {
        var softwareExists = await _db.Softwares.AnyAsync(s => s.Id == softwareId);
        if (!softwareExists)
            return null;

        var softwareVersion = new SoftwareVersion
        {
            SoftwareId = softwareId,
            OsId = osId,
            VersionNumber = versionNumber,
            InstallationDetails = installationDetails,
            Notes = notes,
        };

        _db.SoftwareVersions.Add(softwareVersion);
        await _db.SaveChangesAsync();

        return softwareVersion;
    }

    public async Task<List<SoftwareVersion>> GetAllAsync()
    {
        return await _db.SoftwareVersions.ToListAsync();
    }

    public async Task<List<SoftwareVersion>> GetAllBySoftwareIdAsync(int softwareId)
    {
        return await _db.SoftwareVersions
            .Where(x => x.SoftwareId == softwareId)
            .ToListAsync();
    }

    public async Task<SoftwareVersion?> GetByIdAsync(int id)
    {
        return await _db.SoftwareVersions.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(int id, int osId, string versionNumber, string? installationDetails, string? notes)
    {
        var softwareVersion = await _db.SoftwareVersions.FindAsync(id);
        if (softwareVersion is null)
            return false;

        softwareVersion.OsId = osId;
        softwareVersion.VersionNumber = versionNumber;
        softwareVersion.InstallationDetails = installationDetails;
        softwareVersion.Notes = notes;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var softwareVersion = await _db.SoftwareVersions.FindAsync(id);
        if (softwareVersion is null)
            return false;

        _db.SoftwareVersions.Remove(softwareVersion);
        await _db.SaveChangesAsync();

        return true;
    }
}