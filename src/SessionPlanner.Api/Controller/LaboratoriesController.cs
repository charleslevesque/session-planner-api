using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Dtos.Workstations;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.Laboratories;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Laboratories")]
public class LaboratoriesController : ControllerBase
{
    private readonly ILaboratoryService _laboratoryService;

    public LaboratoriesController(ILaboratoryService laboratoryService)
    {
        _laboratoryService = laboratoryService;
    }

    // GET avec filtres (building, capacity)
     /// <summary>
    /// Retrieves all laboratories.
    /// </summary>
    /// <remarks>
    /// Returns all laboratories and supports optional filtering by building and seating capacity.
    /// </remarks>
    /// <param name="building">An optional building name filter.</param>
    /// <param name="minCapacity">An optional minimum seating capacity filter.</param>
    /// <param name="maxCapacity">An optional maximum seating capacity filter.</param>
    /// <returns>A list of laboratories matching the supplied filters.</returns>
    /// <response code="200">The laboratories were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read laboratories.</response>
    [HttpGet]
    [HasPermission(Permissions.Laboratories.Read)]
    [SwaggerOperation(
        Summary = "Get all laboratories",
        Description = "Returns all laboratories, optionally filtered by building and seating capacity."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LaboratoryListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<LaboratoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<LaboratoryResponse>>> GetAll(
        [FromQuery] string? building = null,
        [FromQuery] int? minCapacity = null,
        [FromQuery] int? maxCapacity = null)
    {
        var labs = await _laboratoryService.GetAllAsync(building, minCapacity, maxCapacity);
        return Ok(labs.Select(l => l.ToResponse()));
    }

    /// <summary>
    /// Retrieves a laboratory by identifier.
    /// </summary>
    /// <param name="id">The laboratory identifier.</param>
    /// <returns>The matching laboratory.</returns>
    /// <response code="200">The laboratory was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read laboratories.</response>
    /// <response code="404">No laboratory exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.Laboratories.Read)]
    [SwaggerOperation(
        Summary = "Get a laboratory by id",
        Description = "Returns a single laboratory by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LaboratoryResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(LaboratoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LaboratoryResponse>> GetById(int id)
    {
        var lab = await _laboratoryService.GetByIdAsync(id);

        if (lab is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Laboratory not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No laboratory exists with id {id}."
            ));
        }
        
        return Ok(lab.ToResponse());
    }

    /// <summary>
    /// Creates a laboratory.
    /// </summary>
    /// <param name="request">The laboratory details, including the name, the building, the number of PCs and the seating
    /// capacity.</param>
    /// <returns>The newly created laboratory.</returns>
    /// <response code="201">The laboratory was created successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create laboratories.</response>
    [HttpPost]
    [HasPermission(Permissions.Laboratories.Create)]
    [SwaggerOperation(
        Summary = "Create a laboratory",
        Description = "Creates a new laboratory and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateLaboratoryRequest), typeof(CreateLaboratoryRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(LaboratoryResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(LaboratoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LaboratoryResponse>> Create(CreateLaboratoryRequest request)
    {
        var lab = await _laboratoryService.CreateAsync(
            request.Name,
            request.Building,
            request.NumberOfPCs,
            request.SeatingCapacity);

        return CreatedAtAction(nameof(GetById), new { id = lab.Id }, lab.ToResponse());
    }

    /// <summary>
    /// Updates an existing laboratory.
    /// </summary>
    /// <param name="id">The laboratory identifier.</param>
    /// <param name="request">The updated laboratory data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The laboratory was updated successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update laboratories.</response>
    /// <response code="404">No laboratory exists with the supplied identifier.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.Laboratories.Update)]
    [SwaggerOperation(
        Summary = "Update a laboratory",
        Description = "Updates an existing laboratory by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdateLaboratoryRequest), typeof(UpdateLaboratoryRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateLaboratoryRequest request)
    {
        var updated = await _laboratoryService.UpdateAsync(
            id,
            request.Name,
            request.Building,
            request.NumberOfPCs,
            request.SeatingCapacity);

        if (!updated)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Laboratory not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No laboratory exists with id {id}."
            ));
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a laboratory.
    /// </summary>
    /// <param name="id">The laboratory identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The laboratory was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete laboratories.</response>
    /// <response code="404">No laboratory exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Laboratories.Delete)]
    [SwaggerOperation(
        Summary = "Delete a laboratory",
        Description = "Deletes an existing laboratory by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _laboratoryService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Laboratory not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No laboratory exists with id {id}."
            ));
        }

        return NoContent();
    }

    // POST /api/v1/Laboratories/{id}/workstations - Ajouter OS avec nombre de postes
    /// <summary>
    /// Adds a workstation to a laboratory.
    /// </summary>
    /// <param name="id">The laboratory identifier.</param>
    /// <param name="request">The workstation's name, and the operating system it uses.</param>
    /// <returns>The newly created workstation.</returns>
    /// <response code="201">The workstation was added successfully.</response>
    /// <response code="400">The supplied operating system does not exist.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">The laboratory was not found.</response>
    [HttpPost("{id}/workstations")]
    [HasPermission(Permissions.Workstations.Create)]
    [SwaggerOperation(
        Summary = "Add a workstation to a laboratory",
        Description = "Creates a workstation in the specified laboratory."
    )]
    [SwaggerRequestExample(typeof(AddWorkstationRequest), typeof(AddWorkstationRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(WorkstationResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(WorkstationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkstationResponse>> AddWorkstation(int id, [FromBody] AddWorkstationRequest request)
    {
        var result = await _laboratoryService.AddWorkstationAsync(id, request.Name, request.OSId);

        if (result.Status == AddWorkstationStatus.LaboratoryNotFound)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Laboratory not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No laboratory exists with id {id}."
            ));
        }

        if (result.Status == AddWorkstationStatus.OperatingSystemNotFound)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Operating system not found.",
                Code: ErrorCodes.BadRequest,
                Details: $"No operating system exists with id {request.OSId}."
            ));
        }

        return CreatedAtAction(nameof(GetById), new { id }, result.Workstation!.ToResponse());
    }

    // DELETE /api/v1/Laboratories/{id}/workstations/{osId} - Retirer OS
    /// <summary>
    /// Removes a workstation from a laboratory.
    /// </summary>
    /// <param name="id">The laboratory identifier.</param>
    /// <param name="workstationId">The workstation identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The workstation was removed successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">The laboratory or workstation was not found.</response>
    [HttpDelete("{id}/workstations/{workstationId}")]
    [SwaggerOperation(
        Summary = "Remove a workstation from a laboratory",
        Description = "Removes a workstation from the specified laboratory."
    )]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveWorkstation(int id, int workstationId)
    {
        var removed = await _laboratoryService.RemoveWorkstationAsync(id, workstationId);
        if (!removed)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Laboratory or workstation not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No workstation exists with id {workstationId} in laboratory {id}."
            ));
        }
        return NoContent();
    }
}
