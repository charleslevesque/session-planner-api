namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record TeachingNeedItemResponse(
    int Id,
    int? SoftwareId,
    string? SoftwareName,
    int? SoftwareVersionId,
    string? SoftwareVersionNumber,
    int? OSId,
    string? OSName,
    int? Quantity,
    string? Notes);
