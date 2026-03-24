using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Api.Dtos.OperatingSystems;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.OperatingSystems;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Operating Systems")]
public class OperatingSystemsController : ControllerBase
{
    private readonly IOperatingSystemService _operatingSystemService;

    public OperatingSystemsController(IOperatingSystemService operatingSystemService)
    {
        _operatingSystemService = operatingSystemService;
    }

    /// <summary>
    /// Retrieves all operating systems.
    /// </summary>
    /// <returns>A list of operating systems.</returns>
    /// <response code="200">The operating systems were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read operating systems.</response>
    [HttpGet]
    [HasPermission(Permissions.OperatingSystems.Read)]
    [SwaggerOperation(
        Summary = "Get all operating systems",
        Description = "Returns all operating systems."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(OSListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<OSResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<OSResponse>>> GetAll()
    {
        var osList = await _operatingSystemService.GetAllAsync();
        return Ok(osList.Select(os => os.ToResponse()));
    }
    
    /// <summary>
    /// Retrieves an operating system by identifier.
    /// </summary>
    /// <param name="id">The operating system identifier.</param>
    /// <returns>The matching operating system.</returns>
    /// <response code="200">The operating system was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read operating systems.</response>
    /// <response code="404">No operating system exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.OperatingSystems.Read)]
    [SwaggerOperation(
        Summary = "Get an operating system by id",
        Description = "Returns a single operating system by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(OSResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(OSResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OSResponse>> GetById(int id)
    {
        var os = await _operatingSystemService.GetByIdAsync(id);
        if (os is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Operating system not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No operating system exists with id {id}."
            ));
        }
        return Ok(os.ToResponse());
    }

    /// <summary>
    /// Creates an operating system.
    /// </summary>
    /// <param name="request">The operating system details.</param>
    /// <returns>The newly created operating system.</returns>
    /// <response code="201">The operating system was created successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create operating systems.</response>
    [HttpPost]
    [HasPermission(Permissions.OperatingSystems.Create)]
    [SwaggerOperation(
        Summary = "Create an operating system",
        Description = "Creates a new operating system and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateOSRequest), typeof(CreateOSRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(OSResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(OSResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OSResponse>> Create(CreateOSRequest request)
    {
        var os = await _operatingSystemService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetById), new { id = os.Id }, os.ToResponse());
    }

    /// <summary>
    /// Updates an existing operating system.
    /// </summary>
    /// <param name="id">The operating system identifier.</param>
    /// <param name="request">The updated operating system data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The operating system was updated successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update operating systems.</response>
    /// <response code="404">No operating system exists with the supplied identifier.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.OperatingSystems.Update)]
    [SwaggerOperation(
        Summary = "Update an operating system",
        Description = "Updates an existing operating system by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdateOSRequest), typeof(UpdateOSRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateOSRequest request)
    {
        var updated = await _operatingSystemService.UpdateAsync(id, request.Name);
        if (!updated)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Operating system not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No operating system exists with id {id}."
            ));
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes an operating system.
    /// </summary>
    /// <param name="id">The operating system identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The operating system was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete operating systems.</response>
    /// <response code="404">No operating system exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.OperatingSystems.Delete)]
    [SwaggerOperation(
        Summary = "Delete an operating system",
        Description = "Deletes an existing operating system by its identifier."
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
        var deleted = await _operatingSystemService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Operating system not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No operating system exists with id {id}."
            ));
        }
        return NoContent();
    }
}
