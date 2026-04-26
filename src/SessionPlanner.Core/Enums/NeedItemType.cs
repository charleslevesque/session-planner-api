using System.Text.Json;
using System.Text.Json.Serialization;

namespace SessionPlanner.Core.Enums;

/// <summary>
/// Converts <see cref="NeedItemType"/> to and from its snake_case string representation
/// (e.g. "virtual_machine"), matching the convention used by the frontend and stored in the database.
/// Integer values are explicitly rejected — only the defined snake_case strings are accepted.
/// </summary>
public sealed class NeedItemTypeJsonConverter : JsonConverter<NeedItemType>
{
    public override NeedItemType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            throw new JsonException("Integer values are not allowed for NeedItemType. Use a snake_case string (e.g. \"virtual_machine\").");

        var raw = reader.GetString()
            ?? throw new JsonException("NeedItemType cannot be null.");

        return NeedItemTypeExtensions.FromSnakeCase(raw);
    }

    public override void Write(Utf8JsonWriter writer, NeedItemType value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToSnakeCase());
}

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

public static class NeedItemTypeExtensions
{
    /// <summary>Converts the enum value to its snake_case string representation (e.g. "virtual_machine").</summary>
    public static string ToSnakeCase(this NeedItemType value)
        => JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString());

    /// <summary>
    /// Parses a snake_case string (e.g. "virtual_machine") back to a <see cref="NeedItemType"/>.
    /// Throws <see cref="JsonException"/> for unknown or numeric strings.
    /// </summary>
    public static NeedItemType FromSnakeCase(string value)
    {
        // Strip underscores and do a case-insensitive match to the enum member name.
        var normalized = value.Replace("_", "");

        // Reject numeric strings outright — Enum.TryParse would accept "0", "999", etc.
        if (int.TryParse(value, out _))
            throw new JsonException($"Integer string \"{value}\" is not a valid NeedItemType. Use a snake_case string (e.g. \"virtual_machine\").");

        if (!Enum.TryParse<NeedItemType>(normalized, ignoreCase: true, out var result) || !Enum.IsDefined(result))
            throw new JsonException($"\"{value}\" is not a valid NeedItemType.");

        return result;
    }
}
