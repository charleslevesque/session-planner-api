namespace SessionPlanner.Web.Models;

public record AiStatusResponse(bool Available);
public record AiSuggestRequest(string CourseCode, List<TeachingNeedItemResponse> Items, List<NeedHistoryEntry> History);
public record AiSuggestResponse(List<TeachingNeedItemResponse> SuggestedItems, string? Explanation);
public record AiReviewResponse(string Review, List<string> Warnings, bool Approved);
