namespace SessionPlanner.Core.Interfaces;

public interface IAiSuggestionService
{
    Task<AiSuggestionsResult> SuggestItemsAsync(int sessionId, int courseId, string? itemType = null);
    Task<AiReviewAnalysis> AnalyzeNeedForReviewAsync(int sessionId, int needId);
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
