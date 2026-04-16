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

public record AiAnalyzeRequest(int SessionId, int NeedId);

public record AiReviewAnalysisDto(
    string Summary,
    IReadOnlyList<string> Alerts,
    string? SuggestedAction,
    string? DraftRejectReason,
    IReadOnlyList<AiHistoryComparisonDto> HistoryComparisons);

public record AiHistoryComparisonDto(string SessionTitle, string Similarity);

public record AutoFillRequestDto(
    int SessionId,
    int CourseId,
    string ItemType,
    Dictionary<string, string> CurrentValues);

public record AutoFillResponseDto(
    Dictionary<string, AutoFillSuggestionDto> Suggestions,
    string Source);

public record AutoFillSuggestionDto(
    string Value,
    string Reason,
    float Confidence);
