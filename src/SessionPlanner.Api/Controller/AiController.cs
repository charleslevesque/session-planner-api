using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Dtos.Ai;
using SessionPlanner.Api.Dtos.Common;
using Swashbuckle.AspNetCore.Annotations;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("AI")]
public class AiController : ControllerBase
{
    private readonly IAiSuggestionService _aiService;

    public AiController(IAiSuggestionService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet("status")]
    [SwaggerOperation(Summary = "Check if AI suggestions are available")]
    public IActionResult GetStatus()
    {
        return Ok(new { available = _aiService.IsConfigured });
    }

    [HttpPost("suggest-items")]
    [SwaggerOperation(
        Summary = "Get AI-powered item suggestions for a teaching need",
        Description = "Returns suggested software/resources based on course history, catalog, and current session context.")]
    [ProducesResponseType(typeof(AiSuggestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SuggestItems([FromBody] AiSuggestRequest request)
    {
        if (!_aiService.IsConfigured)
            return StatusCode(503, new ApiErrorResponse("AI suggestions are not configured. Contact the administrator.", "AI_NOT_CONFIGURED"));

        var result = await _aiService.SuggestItemsAsync(request.SessionId, request.CourseId, request.ItemType);

        var response = new AiSuggestResponse(
            Suggestions: result.Suggestions.Select(s => new AiSuggestedItemDto(
                s.ItemType, s.Label, s.SoftwareName, s.Version,
                s.Os, s.InstallCommand, s.Notes, s.Reason
            )).ToList(),
            Summary: result.Summary);

        return Ok(response);
    }

    [HttpPost("analyze-need")]
    [SwaggerOperation(
        Summary = "Get AI analysis of a teaching need for admin review",
        Description = "Analyzes a teaching need against course history, catalog, and lab matrix to help the admin decide whether to approve or reject.")]
    [ProducesResponseType(typeof(AiReviewAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AnalyzeNeed([FromBody] AiAnalyzeRequest request)
    {
        if (!_aiService.IsConfigured)
            return StatusCode(503, new ApiErrorResponse("AI analysis is not configured. Contact the administrator.", "AI_NOT_CONFIGURED"));

        var result = await _aiService.AnalyzeNeedForReviewAsync(request.SessionId, request.NeedId);

        var response = new AiReviewAnalysisDto(
            Summary: result.Summary,
            Alerts: result.Alerts.ToList(),
            SuggestedAction: result.SuggestedAction,
            DraftRejectReason: result.DraftRejectReason,
            HistoryComparisons: result.HistoryComparisons.Select(c =>
                new AiHistoryComparisonDto(c.SessionTitle, c.Similarity)).ToList());

        return Ok(response);
    }

    [HttpPost("auto-fill")]
    [SwaggerOperation(
        Summary = "Get auto-fill suggestions for form fields",
        Description = "Returns suggested values for empty fields based on course history and software catalog. Does not require OpenAI — works with local data.")]
    [ProducesResponseType(typeof(AutoFillResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AutoFill([FromBody] AutoFillRequestDto request)
    {
        var coreRequest = new AutoFillRequest(
            request.SessionId, request.CourseId, request.ItemType, request.CurrentValues);

        var result = await _aiService.AutoFillFieldsAsync(coreRequest);

        var response = new AutoFillResponseDto(
            Suggestions: result.Suggestions.ToDictionary(
                kv => kv.Key,
                kv => new AutoFillSuggestionDto(kv.Value.Value, kv.Value.Reason, kv.Value.Confidence)),
            Source: result.Source);

        return Ok(response);
    }
}
