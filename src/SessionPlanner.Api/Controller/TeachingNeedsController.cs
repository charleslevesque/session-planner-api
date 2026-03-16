using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Asp.Versioning;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Auth;
using SessionPlanner.Api.Dtos.TeachingNeeds;
using SessionPlanner.Api.Mappings;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/sessions/{sessionId:int}/needs")]
[ApiVersion("1.0")]
[Authorize]
public class TeachingNeedsController : ControllerBase
{
    private readonly ITeachingNeedService _needService;

    public TeachingNeedsController(ITeachingNeedService needService)
    {
        _needService = needService;
    }

    // GET /api/v1/sessions/{sessionId}/needs
    // Tous les rôles — filtré : Teacher voit seulement ses propres besoins
    [HttpGet]
    [HasPermission(Permissions.TeachingNeeds.Read)]
    public async Task<ActionResult<IEnumerable<TeachingNeedResponse>>> GetAll(int sessionId)
    {
        int? filterByPersonnelId = null;

        if (User.IsInRole(Roles.Teacher))
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            filterByPersonnelId = await _needService.GetPersonnelIdForUserAsync(userId.Value);
        }

        var needs = await _needService.GetAllBySessionAsync(sessionId, filterByPersonnelId);
        return Ok(needs.Select(n => n.ToResponse()));
    }

    // GET /api/v1/sessions/{sessionId}/needs/{id}
    // Propriétaire, Admin, RespTech
    [HttpGet("{id:int}")]
    [HasPermission(Permissions.TeachingNeeds.Read)]
    public async Task<ActionResult<TeachingNeedResponse>> GetById(int sessionId, int id)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null) return NotFound();

        // Teacher can only view their own need
        if (User.IsInRole(Roles.Teacher) && !await IsOwner(need.PersonnelId))
            return Forbid();

        return Ok(need.ToResponse());
    }

    // POST /api/v1/sessions/{sessionId}/needs
    // Enseignant, Admin, RespTech
    [HttpPost]
    [HasPermission(Permissions.TeachingNeeds.Create)]
    public async Task<ActionResult<TeachingNeedResponse>> Create(int sessionId, CreateTeachingNeedRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        int personnelId;

        if (User.IsInRole(Roles.Teacher))
        {
            var ownPersonnelId = await _needService.GetPersonnelIdForUserAsync(userId.Value);
            if (ownPersonnelId is null)
                return BadRequest(new { error = "Your account is not linked to any personnel record." });
            personnelId = ownPersonnelId.Value;
        }
        else
        {
            if (request.PersonnelId is null)
                return BadRequest(new { error = "personnelId is required for non-Teacher users." });
            personnelId = request.PersonnelId.Value;
        }

        try
        {
            var need = await _needService.CreateAsync(sessionId, personnelId, request.CourseId, request.Notes);
            return CreatedAtAction(nameof(GetById), new { sessionId, id = need.Id }, need.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // PUT /api/v1/sessions/{sessionId}/needs/{id}
    // Propriétaire uniquement (si statut Draft ou Rejected)
    [HttpPut("{id:int}")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Update(int sessionId, int id, UpdateTeachingNeedRequest request)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null) return NotFound();

        if (!await IsOwner(need.PersonnelId)) return Forbid();

        try
        {
            var updated = await _needService.UpdateAsync(sessionId, id, request.CourseId, request.Notes);
            if (updated is null) return NotFound();
            return Ok(updated.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // DELETE /api/v1/sessions/{sessionId}/needs/{id}
    // Propriétaire uniquement (si statut Draft)
    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.TeachingNeeds.Delete)]
    public async Task<IActionResult> Delete(int sessionId, int id)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null) return NotFound();

        if (!await IsOwner(need.PersonnelId)) return Forbid();

        try
        {
            var deleted = await _needService.DeleteAsync(sessionId, id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
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
    [HttpDelete("{id:int}/items/{itemId:int}")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<IActionResult> RemoveItem(int sessionId, int id, int itemId)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null) return NotFound();

        if (!await IsOwner(need.PersonnelId)) return Forbid();

        try
        {
            var deleted = await _needService.RemoveItemAsync(sessionId, id, itemId);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // POST /api/v1/sessions/{sessionId}/needs/{id}/submit
    // Enseignant : Draft -> Submitted
    [HttpPost("{id:int}/submit")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Submit(int sessionId, int id)
    {
        if (!User.IsInRole(Roles.Teacher)) return Forbid();

        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null) return NotFound();
        if (!await IsOwner(need.PersonnelId)) return Forbid();

        try
        {
            var submitted = await _needService.SubmitAsync(sessionId, id);
            if (submitted is null) return NotFound();
            return Ok(submitted.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // POST /api/v1/sessions/{sessionId}/needs/{id}/review
    // RespTech/Admin : Submitted -> UnderReview
    [HttpPost("{id:int}/review")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Review(int sessionId, int id)
    {
        if (!IsAdminOrTechnician()) return Forbid();

        try
        {
            var reviewed = await _needService.ReviewAsync(sessionId, id);
            if (reviewed is null) return NotFound();
            return Ok(reviewed.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // POST /api/v1/sessions/{sessionId}/needs/{id}/approve
    // RespTech/Admin : UnderReview -> Approved
    [HttpPost("{id:int}/approve")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Approve(int sessionId, int id)
    {
        if (!IsAdminOrTechnician()) return Forbid();

        var currentUserId = GetCurrentUserId();

        try
        {
            var approved = await _needService.ApproveAsync(sessionId, id, currentUserId);
            if (approved is null) return NotFound();
            return Ok(approved.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // POST /api/v1/sessions/{sessionId}/needs/{id}/reject
    // RespTech/Admin : UnderReview -> Rejected
    [HttpPost("{id:int}/reject")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Reject(int sessionId, int id, RejectTeachingNeedRequest request)
    {
        if (!IsAdminOrTechnician()) return Forbid();

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { error = "reason is required." });

        var currentUserId = GetCurrentUserId();

        try
        {
            var rejected = await _needService.RejectAsync(sessionId, id, request.Reason.Trim(), currentUserId);
            if (rejected is null) return NotFound();
            return Ok(rejected.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // POST /api/v1/sessions/{sessionId}/needs/{id}/revise
    // Enseignant : Rejected -> Draft
    [HttpPost("{id:int}/revise")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    public async Task<ActionResult<TeachingNeedResponse>> Revise(int sessionId, int id)
    {
        if (!User.IsInRole(Roles.Teacher)) return Forbid();

        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null) return NotFound();
        if (!await IsOwner(need.PersonnelId)) return Forbid();

        try
        {
            var revised = await _needService.ReviseAsync(sessionId, id);
            if (revised is null) return NotFound();
            return Ok(revised.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
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
