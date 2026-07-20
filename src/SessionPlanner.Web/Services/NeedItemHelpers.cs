using System.Text.Json;
using SessionPlanner.Web.Models;

namespace SessionPlanner.Web.Services;

public static class NeedItemHelpers
{
    public static readonly Dictionary<string, Dictionary<string, string>> Defaults = new()
    {
        [NeedItemTypes.SaaS] = new() { ["name"] = "", ["numberOfAccounts"] = "", ["notes"] = "" },
        [NeedItemTypes.Software] = new() { ["softwareName"] = "", ["versionNumber"] = "", ["osId"] = "", ["installationDetails"] = "", ["notes"] = "" },
        [NeedItemTypes.Configuration] = new() { ["title"] = "", ["osIds"] = "", ["laboratoryIds"] = "", ["notes"] = "" },
        [NeedItemTypes.VirtualMachine] = new() { ["quantity"] = "1", ["cpuCores"] = "", ["ramGb"] = "", ["storageGb"] = "", ["accessType"] = "", ["osId"] = "", ["hostServerId"] = "", ["notes"] = "" },
        [NeedItemTypes.PhysicalServer] = new() { ["hostname"] = "", ["cpuCores"] = "", ["ramGb"] = "", ["storageGb"] = "", ["accessType"] = "", ["osId"] = "", ["notes"] = "" },
        [NeedItemTypes.EquipmentLoan] = new() { ["name"] = "", ["quantity"] = "1", ["defaultAccessories"] = "", ["notes"] = "" }
    };

    public static Dictionary<string, string> ParseDetails(string? detailsJson)
    {
        if (string.IsNullOrWhiteSpace(detailsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(detailsJson, JsonDefaults.Options) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static string SerializeDetails(Dictionary<string, string> values) =>
        JsonSerializer.Serialize(values, JsonDefaults.Options);

    public static string Summary(TeachingNeedItemResponse item, IReadOnlyList<OSResponse> osList, IReadOnlyList<LaboratoryResponse> labs)
    {
        var values = ParseDetails(item.DetailsJson);
        string Get(string key) => values.TryGetValue(key, out var value) ? value : "";

        return item.ItemType switch
        {
            NeedItemTypes.SaaS => Join(Get("name"), WithSuffix(Get("numberOfAccounts"), " comptes")),
            NeedItemTypes.Software => Join(Get("softwareName"), Prefix(Get("versionNumber"), "v"), LookupName(Get("osId"), osList)),
            NeedItemTypes.Configuration => Join(Get("title"), LookupNames(Get("osIds"), osList), LookupLabNames(Get("laboratoryIds"), labs)),
            NeedItemTypes.VirtualMachine => Join(WithSuffix(Get("quantity"), "x"), WithSuffix(Get("cpuCores"), " coeurs"), WithSuffix(Get("ramGb"), " Go RAM"), LookupName(Get("osId"), osList)),
            NeedItemTypes.PhysicalServer => Join(Get("hostname"), WithSuffix(Get("cpuCores"), " coeurs"), WithSuffix(Get("ramGb"), " Go RAM"), LookupName(Get("osId"), osList)),
            NeedItemTypes.EquipmentLoan => Join(Get("name"), WithSuffix(Get("quantity"), "x"), Get("defaultAccessories")),
            _ => item.Description ?? item.Notes ?? NeedItemTypes.Label(item.ItemType)
        };
    }

    public static List<int> ParseIds(string? raw) =>
        (raw ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

    public static string JoinIds(IEnumerable<int> ids) => string.Join(",", ids);

    private static string LookupName(string id, IReadOnlyList<OSResponse> osList) =>
        int.TryParse(id, out var osId) ? osList.FirstOrDefault(os => os.Id == osId)?.Name ?? id : "";

    private static string LookupNames(string ids, IReadOnlyList<OSResponse> osList) =>
        Join(ParseIds(ids).Select(id => osList.FirstOrDefault(os => os.Id == id)?.Name ?? id.ToString()).ToArray());

    private static string LookupLabNames(string ids, IReadOnlyList<LaboratoryResponse> labs) =>
        Join(ParseIds(ids).Select(id => labs.FirstOrDefault(lab => lab.Id == id)?.Name ?? id.ToString()).ToArray());

    private static string Prefix(string value, string prefix) => string.IsNullOrWhiteSpace(value) ? "" : $"{prefix}{value}";

    private static string WithSuffix(string value, string suffix) => string.IsNullOrWhiteSpace(value) ? "" : $"{value}{suffix}";

    private static string Join(params string?[] parts) =>
        string.Join(" - ", parts.Where(part => !string.IsNullOrWhiteSpace(part))).Trim();
}
