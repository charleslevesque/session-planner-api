using System.Text.Json.Serialization;

namespace SessionPlanner.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SessionStatus
{
    Draft = 1,
    Open = 2,
    Closed = 3,
    Archived = 4
}
