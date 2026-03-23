using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.SoftwareVersions;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.SoftwareVersions;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Software Versions")]
public class SoftwareVersionsController : ControllerBase
{

    private readonly ISoftwareVersionService _softwareVersionService;

    public SoftwareVersionsController(ISoftwareVersionService softwareVersionService)
    {
        _softwareVersionService = softwareVersionService;
    }

    /// <summary>
    /// Creates a software version.
    /// </summary>
    /// <param name="request">The software version details, including the associated softwares's identifier, the operating system's identifier, the version number,
    /// the installation details, and the optional notes.</param>
    /// <returns>The newly created software version.</returns>
    /// <response code="201">The software version was created successfully.</response>
    /// <response code="400">The referenced software does not exist.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create software versions.</response>
    [HttpPost]
    [HasPermission(Permissions.SoftwareVersions.Create)]
    [SwaggerOperation(
        Summary = "Create a software version",
        Description = "Creates a new software version and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateSoftwareVersionRequest), typeof(CreateSoftwareVersionRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(SoftwareVersionResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(SoftwareNotFoundForSoftwareVersionExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(SoftwareVersionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SoftwareVersionResponse>> Create(CreateSoftwareVersionRequest request)
    {
        var softwareVersion = await _softwareVersionService.CreateAsync(
            request.SoftwareId,
            request.OsId,
            request.VersionNumber,
            request.InstallationDetails,
            request.Notes);

        if (softwareVersion is null)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Software not found.",
                Code: ErrorCodes.BadRequest,
                Details: $"Software {request.SoftwareId} does not exist."
            ));
        }

        return CreatedAtAction(
            nameof(GetAll),
            new { id = softwareVersion.Id },
            softwareVersion.ToResponse());
    }


    /// <summary>
    /// Retrieves all software versions.
    /// </summary>
    /// <returns>A list of software versions.</returns>
    /// <response code="200">The software versions were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read software versions.</response>
    [HttpGet]
    [HasPermission(Permissions.SoftwareVersions.Read)]
    [SwaggerOperation(
        Summary = "Get all software versions",
        Description = "Returns all software versions."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SoftwareVersionListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<SoftwareVersionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SoftwareVersionResponse>>> GetAll()
    {
        var softwareVersions = await _softwareVersionService.GetAllAsync();
        return Ok(softwareVersions.Select(s => s.ToResponse()));
    }

    /// <summary>
    /// Retrieves a software version by identifier.
    /// </summary>
    /// <param name="id">The software version identifier.</param>
    /// <returns>The matching software version.</returns>
    /// <response code="200">The software version was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read software versions.</response>
    /// <response code="404">No software version exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.SoftwareVersions.Read)]
    [SwaggerOperation(
        Summary = "Get a software version by id",
        Description = "Returns a single software version by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SoftwareVersionResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(SoftwareVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwareVersionResponse>> GetById(int id)
    {
        var softwareVersion = await _softwareVersionService.GetByIdAsync(id);

        if (softwareVersion is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Software version not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No software version exists with id {id}."
            ));
        }

        return Ok(softwareVersion.ToResponse());
    }

    /// <summary>
    /// Retrieves software versions for a specific software product.
    /// </summary>
    /// <param name="softwareId">The software identifier.</param>
    /// <returns>A list of software versions for the supplied software.</returns>
    /// <response code="200">The software versions were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read software or software versions.</response>
    [HttpGet("/api/v{version:apiVersion}/softwares/{softwareId:int}/[controller]")]
    [HasPermission(Permissions.Softwares.Read)]
    [HasPermission(Permissions.SoftwareVersions.Read)]
    [SwaggerOperation(
        Summary = "Get software versions by software id",
        Description = "Returns all software versions associated with a specific software product."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SoftwareVersionListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<SoftwareVersionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SoftwareVersionResponse>>> GetAllBySoftwareId(int softwareId)
    {
        var softwareVersions = await _softwareVersionService.GetAllBySoftwareIdAsync(softwareId);
        var response = softwareVersions.Select(i => i.ToResponse());

        return Ok(response);
    }

    /// <summary>
    /// Updates an existing software version.
    /// </summary>
    /// <param name="id">The software version identifier.</param>
    /// <param name="request">The updated software version data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The software version was updated successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update software versions.</response>
    /// <response code="404">No software version exists with the supplied identifier.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.SoftwareVersions.Update)]
    [SwaggerOperation(
        Summary = "Update a software version",
        Description = "Updates an existing software version by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdateSoftwareVersionRequest), typeof(UpdateSoftwareVersionRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSoftwareVersionRequest request)
    {
        var updated = await _softwareVersionService.UpdateAsync(
            id,
            request.OsId,
            request.VersionNumber,
            request.InstallationDetails,
            request.Notes);

        if (!updated)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Software version not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No software version exists with id {id}."
            ));
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a software version.
    /// </summary>
    /// <param name="id">The software version identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The software version was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete software versions.</response>
    /// <response code="404">No software version exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.SoftwareVersions.Delete)]
    [SwaggerOperation(
        Summary = "Delete a software version",
        Description = "Deletes an existing software version by its identifier."
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
        var deleted = await _softwareVersionService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Software version not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No software version exists with id {id}."
            ));
        }

        return NoContent();
    }
}