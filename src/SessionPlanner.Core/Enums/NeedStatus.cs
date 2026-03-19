using System.Text.Json.Serialization;

namespace SessionPlanner.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NeedStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5
}