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

    /// <summary>
    /// Retrieves all teaching needs for a session.
    /// </summary>
    /// <remarks>
    /// Teachers only see their own needs. Admins and technicians see all.
    /// </remarks>
    // GET /api/v1/sessions/{sessionId}/needs
    // Tous les rôles — filtré : Teacher voit seulement ses propres besoins
    [HttpGet]
    [HasPermission(Permissions.TeachingNeeds.Read)]
    [SwaggerOperation(
        Summary = "Get all teaching needs for a session",
        Description = "Returns all teaching needs for the specified session. Teachers only see their own needs."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedListExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<TeachingNeedResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<TeachingNeedResponse>>> GetAll(int sessionId)
    {
        int? filterByPersonnelId = null;

        if (IsTeachingRole())
        {
            var userId = GetCurrentUserId();
<<<<<<< feature/add-documentation-elements
            if (userId is null)
            {
                return Unauthorized(new ApiErrorResponse(
                    Error: "Unauthorized.",
                    Code: ErrorCodes.Unauthorized
                ));
            }

            filterByPersonnelId = await _needService.GetPersonnelIdForUserAsync(userId.Value);
=======
            if (userId is null) return Unauthorized();

            filterByPersonnelId = await _needService.GetOrCreatePersonnelIdForUserAsync(userId.Value);
            if (filterByPersonnelId is null)
                return Ok(Array.Empty<TeachingNeedResponse>());
>>>>>>> main
        }

        var needs = await _needService.GetAllBySessionAsync(sessionId, filterByPersonnelId);
        return Ok(needs.Select(n => n.ToResponse()));
    }

    /// <summary>
    /// Retrieves a teaching need by identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <returns>The matching teaching need.</returns>
    /// <response code="200">The teaching need was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to access this teaching need.</response>
    /// <response code="404">No teaching need exists with the supplied identifiers.</response>
    // GET /api/v1/sessions/{sessionId}/needs/{id}
    // Propriétaire, Admin, RespTech
    [HttpGet("{id:int}")]
    [HasPermission(Permissions.TeachingNeeds.Read)]
    [SwaggerOperation(
        Summary = "Get a teaching need by id",
        Description = "Returns a single teaching need by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(TeachingNeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeachingNeedResponse>> GetById(int sessionId, int id)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);

<<<<<<< feature/add-documentation-elements
        if (need is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Teaching need not found.",
                Code: ErrorCodes.NotFound
            ));
        }

        if (User.IsInRole(Roles.Teacher) && !await IsOwner(need.PersonnelId))
=======
        if (IsTeachingRole() && !await IsOwner(need.PersonnelId))
>>>>>>> main
            return Forbid();

        return Ok(need.ToResponse());
    }

    /// <summary>
    /// Creates a teaching need.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="request">The teaching need details.</param>
    /// <returns>The newly created teaching need.</returns>
    /// <response code="201">The teaching need was created successfully.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create teaching needs.</response>
    /// <response code="409">The teaching need could not be created because of a business rule conflict.</response>
    // POST /api/v1/sessions/{sessionId}/needs
    // Enseignant, Admin, RespTech
    [HttpPost]
    [HasPermission(Permissions.TeachingNeeds.Create)]
    [SwaggerOperation(
        Summary = "Create a teaching need",
        Description = "Creates a new teaching need for the specified session."
    )]
    [SwaggerRequestExample(typeof(CreateTeachingNeedRequest), typeof(CreateTeachingNeedRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(TeachingNeedResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(InvalidTeachingNeedExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    [ProducesResponseType(typeof(TeachingNeedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeachingNeedResponse>> Create(int sessionId, CreateTeachingNeedRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(
                Error: "Unauthorized.",
                Code: ErrorCodes.Unauthorized
            ));
        }

        int personnelId;

        if (IsTeachingRole())
        {
<<<<<<< feature/add-documentation-elements
            var ownPersonnelId = await _needService.GetPersonnelIdForUserAsync(userId.Value);

=======
            var ownPersonnelId = await _needService.GetOrCreatePersonnelIdForUserAsync(userId.Value);
>>>>>>> main
            if (ownPersonnelId is null)
            {
                return BadRequest(new ApiErrorResponse(
                    Error: "Your account is not linked to any personnel record.",
                    Code: ErrorCodes.BadRequest
                ));
            }

            personnelId = ownPersonnelId.Value;
        }
        else
        {
            if (request.PersonnelId is null)
<<<<<<< feature/add-documentation-elements
            {
                return BadRequest(new ApiErrorResponse(
                    Error: "personnelId is required.",
                    Code: ErrorCodes.BadRequest
                ));
            }

=======
                return BadRequest(new { error = "personnelId is required for non-teaching-role users." });
>>>>>>> main
            personnelId = request.PersonnelId.Value;
        }

        try
        {
            var need = await _needService.CreateAsync(sessionId, personnelId, request.CourseId, request.Notes,
                request.ExpectedStudents, request.HasTechNeeds, request.FoundAllCourses,
                request.DesiredModifications, request.AllowsUpdates, request.AdditionalComments);
            return CreatedAtAction(nameof(GetById), new { sessionId, id = need.Id }, need.ToResponse());
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
    /// Updates a teaching need.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <param name="request">The updated teaching need data.</param>
    /// <returns>The updated teaching need.</returns>
    /// <response code="200">The teaching need was updated successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update this teaching need.</response>
    /// <response code="404">No teaching need exists with the supplied identifiers.</response>
    /// <response code="409">The teaching need could not be updated because of a business rule conflict.</response>
    // PUT /api/v1/sessions/{sessionId}/needs/{id}
    // Propriétaire uniquement (si statut Draft ou Rejected)
    [HttpPut("{id:int}")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(
        Summary = "Update a teaching need",
        Description = "Updates an existing teaching need."
    )]
    [SwaggerRequestExample(typeof(UpdateTeachingNeedRequest), typeof(UpdateTeachingNeedRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    [ProducesResponseType(typeof(TeachingNeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeachingNeedResponse>> Update(int sessionId, int id, UpdateTeachingNeedRequest request)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);

        if (need is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Teaching need not found.",
                Code: ErrorCodes.NotFound
            ));
        }

        if (!await IsOwner(need.PersonnelId))
            return Forbid();

        try
        {
<<<<<<< feature/add-documentation-elements
            var updated = await _needService.UpdateAsync(sessionId, id, request.CourseId, request.Notes);

            if (updated is null)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Teaching need not found.",
                    Code: ErrorCodes.NotFound
                ));
            }

=======
            var updated = await _needService.UpdateAsync(sessionId, id, request.CourseId, request.Notes,
                request.ExpectedStudents, request.HasTechNeeds, request.FoundAllCourses,
                request.DesiredModifications, request.AllowsUpdates, request.AdditionalComments);
            if (updated is null) return NotFound();
>>>>>>> main
            return Ok(updated.ToResponse());
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
    /// Deletes a teaching need.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The teaching need was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete this teaching need.</response>
    /// <response code="404">No teaching need exists with the supplied identifiers.</response>
    /// <response code="409">The teaching need could not be deleted because of a business rule conflict.</response>
    // DELETE /api/v1/sessions/{sessionId}/needs/{id}
    // Propriétaire uniquement (si statut Draft)
    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.TeachingNeeds.Delete)]
    [SwaggerOperation(
        Summary = "Delete a teaching need",
        Description = "Deletes an existing teaching need."
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
    public async Task<IActionResult> Delete(int sessionId, int id)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);

        if (need is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Teaching need not found.",
                Code: ErrorCodes.NotFound
            ));
        }

        if (!await IsOwner(need.PersonnelId))
            return Forbid();

        try
        {
            var deleted = await _needService.DeleteAsync(sessionId, id);

            if (!deleted)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Teaching need not found.",
                    Code: ErrorCodes.NotFound
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
    /// Adds an item to a teaching need.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <param name="request">The item details.</param>
    /// <returns>The newly created teaching need item.</returns>
    /// <response code="201">The item was added successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to modify this teaching need.</response>
    /// <response code="404">The teaching need or a referenced resource was not found.</response>
    /// <response code="409">The item could not be added because of a business rule conflict.</response>
    // POST /api/v1/sessions/{sessionId}/needs/{id}/items
    // Propriétaire uniquement (si statut Draft)
    [HttpPost("{id:int}/items")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(
        Summary = "Add an item to a teaching need",
        Description = "Adds an item to the specified teaching need."
    )]
    [SwaggerRequestExample(typeof(AddNeedItemRequest), typeof(AddTeachingNeedItemRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(TeachingNeedItemExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    [ProducesResponseType(typeof(TeachingNeedItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeachingNeedItemResponse>> AddItem(int sessionId, int id, AddNeedItemRequest request)
    {
        var need = await _needService.GetByIdAsync(sessionId, id);

        if (need is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Teaching need not found.",
                Code: ErrorCodes.NotFound
            ));
        }

        if (!await IsOwner(need.PersonnelId))
            return Forbid();

        try
        {
            var item = await _needService.AddItemAsync(
                sessionId, id,
                request.ItemType ?? "software",
                request.SoftwareId, request.SoftwareVersionId, request.OSId,
                request.Quantity, request.Description, request.Notes);

            if (item is null)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Related entity not found.",
                    Code: ErrorCodes.NotFound
                ));
            }

            return StatusCode(StatusCodes.Status201Created, item.ToResponse());
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
    /// Removes an item from a teaching need.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <param name="itemId">The teaching need item identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The item was removed successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to modify this teaching need.</response>
    /// <response code="404">The teaching need or item was not found.</response>
    /// <response code="409">The item could not be removed because of a business rule conflict.</response>
    // DELETE /api/v1/sessions/{sessionId}/needs/{id}/items/{itemId}
    // Propriétaire uniquement (si statut Draft)
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
                Code: ErrorCodes.NotFound
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
                    Code: ErrorCodes.NotFound
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
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <returns>The updated teaching need.</returns>
    /// <response code="200">The teaching need was submitted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to submit this teaching need.</response>
    /// <response code="404">The teaching need was not found.</response>
    /// <response code="409">The teaching need cannot be submitted from its current state.</response>
    // POST /api/v1/sessions/{sessionId}/needs/{id}/submit
    // Enseignant : Draft -> Submitted
    [HttpPost("{id:int}/submit")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(
        Summary = "Submit a teaching need",
        Description = "Transitions a teaching need from Draft to Submitted."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedResponseExample))]
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
    {
        if (!IsTeachingRole()) return Forbid();

        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Teaching need not found.",
                Code: ErrorCodes.NotFound
            ));
        }

        if (!await IsOwner(need.PersonnelId)) return Forbid();

        try
        {
            var submitted = await _needService.SubmitAsync(sessionId, id);
            if (submitted is null)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Teaching need not found.",
                    Code: ErrorCodes.NotFound
                ));
            }

            return Ok(submitted.ToResponse());
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
    /// Marks a teaching need as under review.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <returns>The updated teaching need.</returns>
    /// <response code="200">The teaching need was moved to review successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to review teaching needs.</response>
    /// <response code="404">The teaching need was not found.</response>
    /// <response code="409">The teaching need cannot be moved to review from its current state.</response>
    // POST /api/v1/sessions/{sessionId}/needs/{id}/review
    // RespTech/Admin : Submitted -> UnderReview
    [HttpPost("{id:int}/review")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(
        Summary = "Review a teaching need",
        Description = "Transitions a teaching need from Submitted to UnderReview."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    [ProducesResponseType(typeof(TeachingNeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeachingNeedResponse>> Review(int sessionId, int id)
    {
        if (!IsAdminOrLabInstructor()) return Forbid();

        try
        {
            var reviewed = await _needService.ReviewAsync(sessionId, id);
            if (reviewed is null)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Teaching need not found.",
                    Code: ErrorCodes.NotFound
                ));
            }

            return Ok(reviewed.ToResponse());
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
    /// Approves a teaching need.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <returns>The updated teaching need.</returns>
    /// <response code="200">The teaching need was approved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to approve teaching needs.</response>
    /// <response code="404">The teaching need was not found.</response>
    /// <response code="409">The teaching need cannot be approved from its current state.</response>
    // POST /api/v1/sessions/{sessionId}/needs/{id}/approve
    // RespTech/Admin : UnderReview -> Approved
    [HttpPost("{id:int}/approve")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(
        Summary = "Approve a teaching need",
        Description = "Transitions a teaching need from UnderReview to Approved."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    [ProducesResponseType(typeof(TeachingNeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeachingNeedResponse>> Approve(int sessionId, int id)
    {
        if (!IsAdminOrLabInstructor()) return Forbid();

        var currentUserId = GetCurrentUserId();

        try
        {
            var approved = await _needService.ApproveAsync(sessionId, id, currentUserId);
            if (approved is null)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Teaching need not found.",
                    Code: ErrorCodes.NotFound
                ));
            }

            return Ok(approved.ToResponse());
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
    /// Rejects a teaching need.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <param name="request">The rejection details.</param>
    /// <returns>The updated teaching need.</returns>
    /// <response code="200">The teaching need was rejected successfully.</response>
    /// <response code="400">The rejection reason is missing or invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to reject teaching needs.</response>
    /// <response code="404">The teaching need was not found.</response>
    /// <response code="409">The teaching need cannot be rejected from its current state.</response>
    // POST /api/v1/sessions/{sessionId}/needs/{id}/reject
    // RespTech/Admin : UnderReview -> Rejected
    [HttpPost("{id:int}/reject")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(
        Summary = "Reject a teaching need",
        Description = "Transitions a teaching need from UnderReview to Rejected."
    )]
    [SwaggerRequestExample(typeof(RejectTeachingNeedRequest), typeof(RejectTeachingNeedRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(InvalidTeachingNeedExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    [ProducesResponseType(typeof(TeachingNeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeachingNeedResponse>> Reject(int sessionId, int id, [FromBody] RejectTeachingNeedRequest request)
    {
        if (!IsAdminOrLabInstructor()) return Forbid();

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Reason is required.",
                Code: ErrorCodes.RejectionReasonRequired
            ));
        }

        var currentUserId = GetCurrentUserId();

        try
        {
            var rejected = await _needService.RejectAsync(sessionId, id, request.Reason.Trim(), currentUserId);
            if (rejected is null)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Teaching need not found.",
                    Code: ErrorCodes.NotFound
                ));
            }

            return Ok(rejected.ToResponse());
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
    /// Revises a rejected teaching need.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="id">The teaching need identifier.</param>
    /// <returns>The updated teaching need.</returns>
    /// <response code="200">The teaching need was revised successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to revise this teaching need.</response>
    /// <response code="404">The teaching need was not found.</response>
    /// <response code="409">The teaching need cannot be revised from its current state.</response>
    // POST /api/v1/sessions/{sessionId}/needs/{id}/revise
    // Enseignant : Rejected -> Draft
    [HttpPost("{id:int}/revise")]
    [HasPermission(Permissions.TeachingNeeds.Update)]
    [SwaggerOperation(
        Summary = "Revise a teaching need",
        Description = "Transitions a teaching need from Rejected to Draft."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeachingNeedResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidTeachingNeedTransitionExample))]
    [ProducesResponseType(typeof(TeachingNeedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeachingNeedResponse>> Revise(int sessionId, int id)
    {
        if (!IsTeachingRole()) return Forbid();

        var need = await _needService.GetByIdAsync(sessionId, id);
        if (need is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Teaching need not found.",
                Code: ErrorCodes.NotFound
            ));
        }

        if (!await IsOwner(need.PersonnelId)) return Forbid();

        try
        {
            var revised = await _needService.ReviseAsync(sessionId, id);
            if (revised is null)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Teaching need not found.",
                    Code: ErrorCodes.NotFound
                ));
            }

            return Ok(revised.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(
                Error: ex.Message,
                Code: ErrorCodes.InvalidTeachingNeedTransition
            ));
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

<<<<<<< feature/add-documentation-elements
    private bool IsAdminOrTechnician() =>
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Technician);

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
        {
            return NotFound(new ApiErrorResponse(
                Error: "Teaching need not found.",
                Code: ErrorCodes.NotFound
            ));
        }

        if (teacherOnly && !await IsOwner(need.PersonnelId))
            return Forbid();

        try
        {
            var result = await action();

            if (result is null)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Teaching need not found.",
                    Code: ErrorCodes.NotFound
                ));
            }

            return Ok(result.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(
                Error: ex.Message,
                Code: ErrorCodes.InvalidTeachingNeedTransition
            ));
        }
    }
}
=======
    private bool IsTeachingRole() =>
        User.IsInRole(Roles.Professor) || User.IsInRole(Roles.CourseInstructor);

    private bool IsAdminOrLabInstructor() =>
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.LabInstructor);
}
>>>>>>> main
