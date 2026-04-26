using SessionPlanner.Core.Enums;

namespace SessionPlanner.Core.Interfaces;

public interface IAiSuggestionService
{
    Task<AiSuggestionsResult> SuggestItemsAsync(int sessionId, int courseId, NeedItemType? itemType = null);
    Task<AiReviewAnalysis> AnalyzeNeedForReviewAsync(int sessionId, int needId);
    Task<AutoFillResult> AutoFillFieldsAsync(AutoFillRequest request);
    Task<RejectionAssistResult> GetRejectionAssistanceAsync(int sessionId, int needId);
    bool IsConfigured { get; }
}

public record AiSuggestedItem(
    NeedItemType ItemType,
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
    NeedItemType ItemType,
    Dictionary<string, string> CurrentValues);

public record AutoFillResult(
    Dictionary<string, AutoFillSuggestion> Suggestions,
    string Source);

public record AutoFillSuggestion(
    string Value,
    string Reason,
    float Confidence);

public record RejectionAssistResult(
    string Explanation,
    IReadOnlyList<RejectionCorrectionStep> Steps,
    string? RevisedNotes);

public record RejectionCorrectionStep(
    string Action,
    string Target,
    string Detail);
