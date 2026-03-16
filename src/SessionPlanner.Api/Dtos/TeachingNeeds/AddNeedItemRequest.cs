namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record AddNeedItemRequest(
    int? SoftwareId,
    int? SoftwareVersionId,
    int? OSId,
    int? Quantity,
    string? Notes);
