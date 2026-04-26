using System.Text.Json.Serialization;

namespace SessionPlanner.Core.Enums;

/// <summary>
/// Converts <see cref="NeedItemType"/> to and from its snake_case string representation
/// (e.g. "virtual_machine"), matching the convention used by the frontend and stored in the database.
/// </summary>
public sealed class NeedItemTypeJsonConverter()
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
