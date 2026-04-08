using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class InstallationCheckService : IInstallationCheckService
{
    private readonly AppDbContext _db;

    public InstallationCheckService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyDictionary<int, bool>> GetInstalledMapAsync(
        IEnumerable<(int itemId, int? softwareId)> items)
    {
        var softwareItems = items
            .Where(x => x.softwareId.HasValue)
            .ToList();

        if (softwareItems.Count == 0)
            return new Dictionary<int, bool>();

        var softwareIds = softwareItems
            .Select(x => x.softwareId!.Value)
            .Distinct()
            .ToList();

        var installedIds = await _db.LaboratorySoftwares
            .Where(ls => softwareIds.Contains(ls.SoftwareId))
            .Select(ls => ls.SoftwareId)
            .Distinct()
            .ToListAsync();

        var installedSet = installedIds.ToHashSet();

        return softwareItems.ToDictionary(
            x => x.itemId,
            x => installedSet.Contains(x.softwareId!.Value));
    }
}
