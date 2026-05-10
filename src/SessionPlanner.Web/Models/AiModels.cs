namespace SessionPlanner.Web.Models;

public record AiStatusResponse(bool Available);
public record AiSuggestRequest(int SessionId, int CourseId, string? ItemType = null);
public record AiSuggestResponse(List<AiSuggestedItemResponse> Suggestions, string? Summary);

public record AiSuggestedItemResponse(
    string ItemType,
    string Label,
    string? SoftwareName,
    string? Version,
    string? Os,
    string? InstallCommand,
    string? Notes,
    string Reason);

public record AiAnalyzeRequest(int SessionId, int NeedId);

public record AiReviewAnalysisResponse(
    string Summary,
    List<string> Alerts,
    string? SuggestedAction,
    string? DraftRejectReason,
    List<AiHistoryComparisonResponse> HistoryComparisons);

public record AiHistoryComparisonResponse(string SessionTitle, string Similarity);

public record AutoFillRequest(int SessionId, int CourseId, string ItemType, Dictionary<string, string> CurrentValues);
public record AutoFillResponse(Dictionary<string, AutoFillSuggestionResponse> Suggestions, string Source);
public record AutoFillSuggestionResponse(string Value, string Reason, float Confidence);

public record RejectionAssistRequest(int SessionId, int NeedId);
public record RejectionAssistResponse(string Explanation, List<CorrectionStepResponse> Steps, string? RevisedNotes);
public record CorrectionStepResponse(string Action, string Target, string Detail);
