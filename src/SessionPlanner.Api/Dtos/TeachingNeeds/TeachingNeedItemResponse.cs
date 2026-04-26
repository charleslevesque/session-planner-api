using SessionPlanner.Core.Enums;

namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record TeachingNeedItemResponse(
    int Id,
    NeedItemType ItemType,
    int? SoftwareId,
    string? SoftwareName,
    int? SoftwareVersionId,
    string? SoftwareVersionNumber,
    int? OSId,
    string? OSName,
    int? Quantity,
    string? Description,
    string? Notes,
    string? DetailsJson,
    bool? AlreadyInstalledInLabs);
