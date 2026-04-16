namespace SessionPlanner.Api.Dtos.Ai;

public record AiSuggestRequest(int SessionId, int CourseId, string? ItemType = null);

public record AiSuggestResponse(
    IReadOnlyList<AiSuggestedItemDto> Suggestions,
    string? Summary);

public record AiSuggestedItemDto(
    string ItemType,
    string Label,
    string? SoftwareName,
    string? Version,
    string? Os,
    string? InstallCommand,
    string? Notes,
    string Reason);
