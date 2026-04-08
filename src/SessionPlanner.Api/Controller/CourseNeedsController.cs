using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Auth;
using SessionPlanner.Api.Dtos.TeachingNeeds;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Common;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/courses/{courseId:int}/needs")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Teaching Needs")]
public class CourseNeedsController : ControllerBase
{
    private readonly ITeachingNeedService _needService;

    public CourseNeedsController(ITeachingNeedService needService)
    {
        _needService = needService;
    }

    /// <summary>
    /// Returns the history of approved teaching needs for a course across all sessions.
    /// </summary>
    // GET /api/v1/courses/{courseId}/needs/history
    [HttpGet("history")]
    [HasPermission(Permissions.TeachingNeeds.Read)]
    [ProducesResponseType(typeof(IEnumerable<TeachingNeedResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<TeachingNeedResponse>>> GetHistory(int courseId)
    {
        var needs = await _needService.GetApprovedHistoryByCourseAsync(courseId);
        return Ok(needs.Select(n => n.ToResponse()));
    }
}
