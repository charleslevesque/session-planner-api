namespace SessionPlanner.Core.Interfaces;

public interface IAiSuggestionService
{
    Task<AiSuggestionsResult> SuggestItemsAsync(int sessionId, int courseId, string? itemType = null);
    Task<AiReviewAnalysis> AnalyzeNeedForReviewAsync(int sessionId, int needId);
    Task<AutoFillResult> AutoFillFieldsAsync(AutoFillRequest request);
    bool IsConfigured { get; }
}

public record AiSuggestedItem(
    string ItemType,
    string Label,
    string? SoftwareName,
    string? Version,
    string? Os,
    string? InstallCommand,
    string? Notes,
    string Reason);

public record AiSuggestionsResult(
    IReadOnlyList<AiSuggestedItem> Suggestions,
    string? Summary);

public record AiReviewAnalysis(
    string Summary,
    IReadOnlyList<string> Alerts,
    string? SuggestedAction,
    string? DraftRejectReason,
    IReadOnlyList<AiHistoryComparison> HistoryComparisons);

public record AiHistoryComparison(string SessionTitle, string Similarity);

public record AutoFillRequest(
    int SessionId,
    int CourseId,
    string ItemType,
    Dictionary<string, string> CurrentValues);

public record AutoFillResult(
    Dictionary<string, AutoFillSuggestion> Suggestions,
    string Source);

public record AutoFillSuggestion(
    string Value,
    string Reason,
    float Confidence);
