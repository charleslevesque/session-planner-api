using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Asp.Versioning;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Auth;
using SessionPlanner.Api.Dtos.TeachingNeeds;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/sessions/{sessionId:int}/needs")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Teaching Needs")]
public class TeachingNeedsController : ControllerBase
{
    private readonly ITeachingNeedService _needService;

    public TeachingNeedsController(ITeachingNeedService needService)
    {
        _needService = needService;
    }

    // GET /api/v1/sessions/{sessionId}/needs
    // Tous les rôles — filtré : Teacher voit seulement ses propres besoins
    /// <summary>
    /// Retrieves all teaching needs for a session.
    /// </summary>
    /// <remarks>
    /// Teachers only see their own needs. Admins and technicians see all.
    /// </remarks>
    [HttpGet]
    [HasPermission(Permissions.TeachingNeeds.Read)]
    [SwaggerOperation(Summary = "Get all teaching needs for a session")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedListExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<TeachingNeedResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TeachingNeedResponse>>> GetAll(int sessionId)
    {
        int? filterByPersonnelId = null;

        if (User.IsInRole(Roles.Teacher))
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(new ApiErrorResponse("Unauthorized.", ErrorCodes.Unauthorized));

            filterByPersonnelId = await _needService.GetPersonnelIdForUserAsync(userId.Value);
        }

        var needs = await _needService.GetAllBySessionAsync(sessionId, filterByPersonnelId);
        return Ok(needs.Select(n => n.ToResponse()));
    }

    // GET /api/v1/sessions/{sessionId}/needs/{id}
    // Propriétaire, Admin, RespTech
    /// <summary>
    /// Retrieves a teaching need by identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    [HasPermission(Permissions.TeachingNeeds.Read)]
    [SwaggerOperation(Summary = "Get a teaching need by id")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(TeachingNeedResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TeachingNeedResponse>> GetById(int sessionId, int id)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null)
            return NotFound(new ApiErrorResponse("Teaching need not found.", ErrorCodes.NotFound));

        if (User.IsInRole(Roles.Teacher) && !await IsOwner(need.PersonnelId))
            return Forbid();

        return Ok(need.ToResponse());
    }

    // POST /api/v1/sessions/{sessionId}/needs
    // Enseignant, Admin, RespTech
    /// <summary>
    /// Creates a teaching need.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.TeachingNeeds.Create)]
    [SwaggerOperation(Summary = "Create a teaching need")]
    [SwaggerRequestExample(typeof(CreateTeachingNeedRequest), typeof(CreateTeachingNeedRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(TeachingNeedExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(InvalidTeachingNeedExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    public async Task<ActionResult<TeachingNeedResponse>> Create(int sessionId, [FromBody] CreateTeachingNeedRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized(new ApiErrorResponse("Unauthorized.", ErrorCodes.Unauthorized));

        int personnelId;

        if (User.IsInRole(Roles.Teacher))
        {
            var ownPersonnelId = await _needService.GetPersonnelIdForUserAsync(userId.Value);

            if (ownPersonnelId is null)
                return BadRequest(new ApiErrorResponse(
                    "Your account is not linked to any personnel record.",
                    ErrorCodes.BadRequest));

            personnelId = ownPersonnelId.Value;
        }
        else
        {
            if (request.PersonnelId is null)
                return BadRequest(new ApiErrorResponse(
                    "personnelId is required.",
                    ErrorCodes.BadRequest));

            personnelId = request.PersonnelId.Value;
        }

        try
        {
            var need = await _needService.CreateAsync(sessionId, personnelId, request.CourseId, request.Notes);

            return CreatedAtAction(nameof(GetById), new { sessionId, id = need.Id }, need.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message, ErrorCodes.InvalidTeachingNeedTransition));
        }
    }

    // PUT /api/v1/sessions/{sessionId}/needs/{id}
    // Propriétaire uniquement (si statut Draft ou Rejected)
    /// <summary>
    /// Updates a teaching need.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(Summary = "Update a teaching need")]
    public async Task<ActionResult<TeachingNeedResponse>> Update(int sessionId, int id, [FromBody] UpdateTeachingNeedRequest request)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null)
            return NotFound(new ApiErrorResponse("Teaching need not found.", ErrorCodes.NotFound));

        if (!await IsOwner(need.PersonnelId))
            return Forbid();

        try
        {
            var updated = await _needService.UpdateAsync(sessionId, id, request.CourseId, request.Notes);

            if (updated is null)
                return NotFound(new ApiErrorResponse("Teaching need not found.", ErrorCodes.NotFound));

            return Ok(updated.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message, ErrorCodes.InvalidTeachingNeedTransition));
        }
    }

    // DELETE /api/v1/sessions/{sessionId}/needs/{id}
    // Propriétaire uniquement (si statut Draft)
    
    /// <summary>
    /// Deletes a teaching need.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.TeachingNeeds.Delete)]
    [SwaggerOperation(Summary = "Delete a teaching need")]
    public async Task<IActionResult> Delete(int sessionId, int id)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null)
            return NotFound(new ApiErrorResponse("Teaching need not found.", ErrorCodes.NotFound));

        if (!await IsOwner(need.PersonnelId))
            return Forbid();

        try
        {
            var deleted = await _needService.DeleteAsync(sessionId, id);

            if (!deleted)
                return NotFound(new ApiErrorResponse("Teaching need not found.", ErrorCodes.NotFound));

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message, ErrorCodes.InvalidTeachingNeedTransition));
        }
    }

    // POST /api/v1/sessions/{sessionId}/needs/{id}/items
    // Propriétaire uniquement (si statut Draft)
    [HttpPost("{id:int}/items")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedItemResponse>> AddItem(int sessionId, int id, AddNeedItemRequest request)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null) return NotFound();

        if (!await IsOwner(need.PersonnelId)) return Forbid();

        try
        {
            var item = await _needService.AddItemAsync(
                sessionId, id,
                request.SoftwareId, request.SoftwareVersionId, request.OSId,
                request.Quantity, request.Notes);

            if (item is null) return NotFound();
            return StatusCode(StatusCodes.Status201Created, item.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // DELETE /api/v1/sessions/{sessionId}/needs/{id}/items/{itemId}
    // Propriétaire uniquement (si statut Draft)
    /// <summary>
    /// Removes an item from a teaching need.
    /// </summary>
    /// <remarks>
    /// Removes an existing item from the specified teaching need.
    /// </remarks>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <param name="itemId">The teaching need item identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The item was removed successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to modify this teaching need.</response>
    /// <response code="404">The teaching need or item was not found.</response>
    /// <response code="409">The item could not be removed because of a business rule conflict.</response>
    [HttpDelete("{id:int}/items/{itemId:int}")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(
        Summary = "Remove an item from a teaching need",
        Description = "Removes an existing item from the specified teaching need."
    )]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveItem(int sessionId, int id, int itemId)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);

        if (need is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Teaching need not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No teaching need exists with id {id} in session {sessionId}."
            ));
        }

        if (!await IsOwner(need.PersonnelId))
            return Forbid();

        try
        {
            var deleted = await _needService.RemoveItemAsync(sessionId, id, itemId);

            if (!deleted)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Teaching need item not found.",
                    Code: ErrorCodes.NotFound,
                    Details: $"No item exists with id {itemId} for teaching need {id}."
                ));
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(
                Error: ex.Message,
                Code: ErrorCodes.InvalidTeachingNeedTransition
            ));
        }
    }

    /// <summary>
    /// Submits a teaching need.
    /// </summary>
    /// <remarks>
    /// Transitions a teaching need from Draft to Submitted.
    /// </remarks>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <returns>The updated teaching need.</returns>
    /// <response code="200">The teaching need was submitted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to submit this teaching need.</response>
    /// <response code="404">The teaching need was not found.</response>
    /// <response code="409">The teaching need cannot be submitted from its current state.</response>
    [HttpPost("{id:int}/submit")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(
        Summary = "Submit a teaching need",
        Description = "Transitions a teaching need from Draft to Submitted."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    [ProducesResponseType(typeof(TeachingNeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeachingNeedResponse>> Submit(int sessionId, int id)
        => await HandleTransition(sessionId, id, () => _needService.SubmitAsync(sessionId, id), teacherOnly: true);

    [HttpPost("{id:int}/review")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Review(int sessionId, int id)
        => await HandleTransition(sessionId, id, () => _needService.ReviewAsync(sessionId, id), adminOnly: true);

    [HttpPost("{id:int}/approve")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Approve(int sessionId, int id)
        => await HandleTransition(sessionId, id, () => _needService.ApproveAsync(sessionId, id, GetCurrentUserId()), adminOnly: true);

    [HttpPost("{id:int}/reject")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Reject(int sessionId, int id, RejectTeachingNeedRequest request)
    {
        if (!IsAdminOrTechnician()) return Forbid();

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new ApiErrorResponse("reason is required.", ErrorCodes.BadRequest));

        try
        {
            var rejected = await _needService.RejectAsync(sessionId, id, request.Reason.Trim(), GetCurrentUserId());
            if (rejected is null)
                return NotFound(new ApiErrorResponse("Teaching need not found.", ErrorCodes.NotFound));

            return Ok(rejected.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message, ErrorCodes.InvalidTeachingNeedTransition));
        }
    }

    [HttpPost("{id:int}/revise")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Revise(int sessionId, int id)
        => await HandleTransition(sessionId, id, () => _needService.ReviseAsync(sessionId, id), teacherOnly: true);

    private async Task<ActionResult<TeachingNeedResponse>> HandleTransition(
        int sessionId,
        int id,
        Func<Task<Core.Entities.TeachingNeed?>> action,
        bool teacherOnly = false,
        bool adminOnly = false)
    {
        if (teacherOnly && !User.IsInRole(Roles.Teacher)) return Forbid();
        if (adminOnly && !IsAdminOrTechnician()) return Forbid();

        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null)
            return NotFound(new ApiErrorResponse("Teaching need not found.", ErrorCodes.NotFound));

        if (teacherOnly && !await IsOwner(need.PersonnelId))
            return Forbid();

        try
        {
            var result = await action();
            if (result is null)
                return NotFound(new ApiErrorResponse("Teaching need not found.", ErrorCodes.NotFound));

            return Ok(result.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(ex.Message, ErrorCodes.InvalidTeachingNeedTransition));
        }
    }

    // --- Helpers ---

    private int? GetCurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private async Task<bool> IsOwner(int needPersonnelId)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return false;
        var personnelId = await _needService.GetPersonnelIdForUserAsync(userId.Value);
        return personnelId == needPersonnelId;
    }

    private bool IsAdminOrTechnician() =>
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Technician);
}
