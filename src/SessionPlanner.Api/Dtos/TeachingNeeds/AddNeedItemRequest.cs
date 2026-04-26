using SessionPlanner.Core.Enums;

namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record AddNeedItemRequest(
    NeedItemType? ItemType,
    int? SoftwareId,
    int? SoftwareVersionId,
    int? OSId,
    int? Quantity,
    string? Description,
    string? Notes,
    string? DetailsJson);
