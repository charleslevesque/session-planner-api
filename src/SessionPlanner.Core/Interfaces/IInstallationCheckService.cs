namespace SessionPlanner.Core.Interfaces;

public interface IInstallationCheckService
{
    /// <summary>
    /// Returns a dictionary mapping TeachingNeedItem.Id -> bool for software-type items,
    /// indicating whether the software is installed in at least one laboratory.
    /// </summary>
    Task<IReadOnlyDictionary<int, bool>> GetInstalledMapAsync(
        IEnumerable<(int itemId, int? softwareId, string? detailsJson)> items);
}
