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
}
