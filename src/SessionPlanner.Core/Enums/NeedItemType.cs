using System.Text.Json.Serialization;

namespace SessionPlanner.Core.Enums;

/// <summary>
/// Converter that serializes NeedItemType as a snake_case string (e.g. "virtual_machine"),
/// matching the convention used by the frontend and stored in the database.
/// </summary>
internal sealed class NeedItemTypeJsonConverter()
    : JsonStringEnumConverter<NeedItemType>(System.Text.Json.JsonNamingPolicy.SnakeCaseLower);

[JsonConverter(typeof(NeedItemTypeJsonConverter))]
public enum NeedItemType
{
    Software,
    Saas,
    VirtualMachine,
    PhysicalServer,
    Configuration,
    EquipmentLoan,
    Other
}
