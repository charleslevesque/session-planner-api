using System.Text.Json;
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
        IEnumerable<(int itemId, int? softwareId, string? detailsJson)> items)
    {
        var softwareItems = items.ToList();

        if (softwareItems.Count == 0)
            return new Dictionary<int, bool>();

        var softwareIds = softwareItems
            .Where(x => x.softwareId.HasValue)
            .Select(x => x.softwareId!.Value)
            .Distinct()
            .ToList();

        var unresolvedByName = softwareItems
            .Where(x => !x.softwareId.HasValue)
            .Select(x => (x.itemId, softwareName: ReadSoftwareName(x.detailsJson)))
            .Where(x => !string.IsNullOrWhiteSpace(x.softwareName))
            .ToList();

        if (unresolvedByName.Count > 0)
        {
            var names = unresolvedByName
                .Select(x => x.softwareName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var softwares = await _db.Softwares
                .Where(s => names.Contains(s.Name))
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var byName = softwares
                .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

            foreach (var unresolved in unresolvedByName)
            {
                if (unresolved.softwareName is null) continue;
                if (byName.TryGetValue(unresolved.softwareName, out var id))
                {
                    softwareIds.Add(id);
                }
            }
        }

        softwareIds = softwareIds.Distinct().ToList();

        if (softwareIds.Count == 0)
            return new Dictionary<int, bool>();

        var installedIds = await _db.LaboratorySoftwares
            .Where(ls => softwareIds.Contains(ls.SoftwareId))
            .Select(ls => ls.SoftwareId)
            .Distinct()
            .ToListAsync();

        var installedSet = installedIds.ToHashSet();

        var resolvedSoftwareIdByItem = new Dictionary<int, int>();

        foreach (var item in softwareItems)
        {
            if (item.softwareId.HasValue)
            {
                resolvedSoftwareIdByItem[item.itemId] = item.softwareId.Value;
                continue;
            }

            var softwareName = ReadSoftwareName(item.detailsJson);
            if (string.IsNullOrWhiteSpace(softwareName))
                continue;

            var matchedId = await _db.Softwares
                .Where(s => s.Name == softwareName)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync();

            if (matchedId.HasValue)
            {
                resolvedSoftwareIdByItem[item.itemId] = matchedId.Value;
            }
        }

        return resolvedSoftwareIdByItem.ToDictionary(
            x => x.Key,
            x => installedSet.Contains(x.Value));
    }

    private static string? ReadSoftwareName(string? detailsJson)
    {
        if (string.IsNullOrWhiteSpace(detailsJson))
            return null;

        try
        {
            using var document = JsonDocument.Parse(detailsJson);
            if (!document.RootElement.TryGetProperty("softwareName", out var element))
                return null;
            var value = element.GetString();
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        catch
        {
            return null;
        }
    }
}
