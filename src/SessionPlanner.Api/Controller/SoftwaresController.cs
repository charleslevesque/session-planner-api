using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Softwares;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Core.Entities;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.Softwares;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Softwares")]
public class SoftwaresController : ControllerBase
{
    private readonly ISoftwareService _softwareService;

    public SoftwaresController(ISoftwareService softwareService)
    {
        _softwareService = softwareService;
    }

    /// <summary>
    /// Returns a standardized catalog of all softwares with their available versions.
    /// </summary>
    /// <remarks>
    /// Use this endpoint to populate software pickers in teaching need forms.
    /// Only softwares that have at least one version are included.
    /// </remarks>
    // GET /api/v1/softwares/catalog
    [HttpGet("catalog")]
    [HasPermission(Permissions.TeachingNeeds.Read)]
    [SwaggerOperation(
        Summary = "Get software catalog",
        Description = "Returns all softwares with their available versions, ordered by name. Use this to populate software pickers."
    )]
    [ProducesResponseType(typeof(IEnumerable<SoftwareCatalogEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SoftwareCatalogEntry>>> GetCatalog()
    {
        var softwares = await _softwareService.GetCatalogAsync();
        return Ok(softwares.Select(s => new SoftwareCatalogEntry(
            s.Id,
            s.Name,
            s.InstallCommand,
            s.SoftwareVersions.Select(v => new SoftwareVersionCatalogEntry(
                v.Id,
                v.VersionNumber,
                v.OsId,
                v.OS?.Name ?? string.Empty,
                v.InstallationDetails,
                v.Notes)))));
    }

    /// <summary>
    /// Creates a software product.
    /// </summary>
    /// <param name="request">The software's name.</param>
    /// <returns>The newly created software product.</returns>
    /// <response code="201">The software was created successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create software products.</response>
    [HttpPost]
    [HasPermission(Permissions.Softwares.Create)]
    [SwaggerOperation(
        Summary = "Create a software product",
        Description = "Creates a new software product and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateSoftwareRequest), typeof(CreateSoftwareRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(SoftwareResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(SoftwareResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SoftwareResponse>> Create(CreateSoftwareRequest request)
    {
        var software = await _softwareService.CreateAsync(request.Name);

        return CreatedAtAction(
            nameof(GetAll),
            new { id = software.Id },
            software.ToResponse());
    }

    /// <summary>
    /// Retrieves all software products.
    /// </summary>
    /// <returns>A list of software products.</returns>
    /// <response code="200">The software products were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read software products.</response>
    [HttpGet]
    [HasPermission(Permissions.Softwares.Read)]
    [SwaggerOperation(
        Summary = "Get all software products",
        Description = "Returns all software products."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SoftwareListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<SoftwareResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SoftwareResponse>>> GetAll()
    {
        var softwares = await _softwareService.GetAllAsync();
        return Ok(softwares.Select(s => s.ToResponse()));
    }

    /// <summary>
    /// Retrieves a software product by identifier.
    /// </summary>
    /// <param name="id">The software identifier.</param>
    /// <returns>The matching software product.</returns>
    /// <response code="200">The software product was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read software products.</response>
    /// <response code="404">No software product exists with the supplied identifier.</response>
    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Softwares.Read)]
    [SwaggerOperation(
        Summary = "Get a software product by id",
        Description = "Returns a single software product by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SoftwareResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(SoftwareResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwareResponse>> GetById(int id)
    {
        var software = await _softwareService.GetByIdAsync(id);
        if (software is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Software product not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No software product exists with id {id}."
            ));
        }
        return Ok(software.ToResponse());
    }

    /// <summary>
    /// Updates an existing software product.
    /// </summary>
    /// <param name="id">The software identifier.</param>
    /// <param name="request">The updated software data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The software product was updated successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update software products.</response>
    /// <response code="404">No software product exists with the supplied identifier.</response>
    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Softwares.Update)]
    [SwaggerOperation(
        Summary = "Update a software product",
        Description = "Updates an existing software product by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdateSoftwareRequest), typeof(UpdateSoftwareRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateSoftwareRequest request)
    {
        var updated = await _softwareService.UpdateAsync(id, request.Name);
        if (!updated)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Software product not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No software product exists with id {id}."
            ));
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Softwares.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _softwareService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Software product not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No software product exists with id {id}."
            ));
        }

        return NoContent();
    }
}