using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.PhysicalServers;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.PhysicalServers;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Physical Server")]
public class PhysicalServersController : ControllerBase
{
    private readonly IPhysicalServerService _physicalServerService;

    public PhysicalServersController(IPhysicalServerService physicalServerService)
    {
        _physicalServerService = physicalServerService;
    }

    /// <summary>
    /// Creates a physical server.
    /// </summary>
    /// <param name="request">The physical server details, including it's host name, the number of cpu cores, the amount of ram (gb), the amount of storage (gb), 
    /// the access type, optional notes and the operating system id.</param>
    /// <returns>The newly created physical server.</returns>
    /// <response code="201">The physical server was created successfully.</response>
    /// <response code="400">The request is invalid, the operating system does not exist, or the hostname is already in use.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create physical servers.</response>
    [HttpPost]
    [HasPermission(Permissions.PhysicalServers.Create)]
    [SwaggerOperation(
        Summary = "Create a physical server",
        Description = "Creates a new physical server and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreatePhysicalServerRequest), typeof(CreatePhysicalServerRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(PhysicalServerResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DuplicatePhysicalServerHostnameExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(PhysicalServerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhysicalServerResponse>> Create(CreatePhysicalServerRequest request)
    {
        var result = await _physicalServerService.CreateAsync(
            request.Hostname,
            request.CpuCores,
            request.RamGb,
            request.StorageGb,
            request.AccessType,
            request.Notes,
            request.OSId);

        if (result.Status == PhysicalServerOperationStatus.OperatingSystemNotFound)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Operating system not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No operating system exists with id {request.OSId}."
            ));
        }

        if (result.Status == PhysicalServerOperationStatus.DuplicateHostname)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "A server with this hostname already exists.",
                Code: ErrorCodes.Conflict,
                Details: $"The hostname '{request.Hostname}' is already assigned to another physical server."
            ));
        }

        var server = result.Server!;

        return CreatedAtAction(
            nameof(GetById),
            new { id = server.Id },
            server.ToResponse());
    }

    /// <summary>
    /// Retrieves all physical servers.
    /// </summary>
    /// <returns>A list of physical servers.</returns>
    /// <response code="200">The physical servers were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read physical servers.</response>
    [HttpGet]
    [HasPermission(Permissions.PhysicalServers.Read)]
    [SwaggerOperation(
        Summary = "Get all physical servers",
        Description = "Returns all physical servers."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PhysicalServerListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<PhysicalServerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PhysicalServerResponse>>> GetAll()
    {
        var servers = await _physicalServerService.GetAllAsync();
        return Ok(servers.Select(s => s.ToResponse()));
    }

    /// <summary>
    /// Retrieves a physical server by identifier.
    /// </summary>
    /// <param name="id">The physical server identifier.</param>
    /// <returns>The matching physical server.</returns>
    /// <response code="200">The physical server was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read physical servers.</response>
    /// <response code="404">No physical server exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.PhysicalServers.Read)]
    [SwaggerOperation(
        Summary = "Get a physical server by id",
        Description = "Returns a single physical server by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PhysicalServerResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(PhysicalServerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhysicalServerResponse>> GetById(int id)
    {
        var server = await _physicalServerService.GetByIdAsync(id);
        if (server is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Physical server not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No physical server exists with id {id}."
            ));
        }
        return Ok(server.ToResponse());
    }

    /// <summary>
    /// Updates an existing physical server.
    /// </summary>
    /// <param name="id">The physical server identifier.</param>
    /// <param name="request">The updated physical server data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The physical server was updated successfully.</response>
    /// <response code="400">The request is invalid, the operating system does not exist, or the hostname is already in use.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update physical servers.</response>
    /// <response code="404">No physical server exists with the supplied identifier.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.PhysicalServers.Update)]
    [SwaggerOperation(
        Summary = "Update a physical server",
        Description = "Updates an existing physical server by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdatePhysicalServerRequest), typeof(UpdatePhysicalServerRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DuplicatePhysicalServerHostnameExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdatePhysicalServerRequest request)
    {
        var status = await _physicalServerService.UpdateAsync(
            id,
            request.Hostname,
            request.CpuCores,
            request.RamGb,
            request.StorageGb,
            request.AccessType,
            request.Notes,
            request.OSId);

        if (status == PhysicalServerOperationStatus.NotFound)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Physical server not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No physical server exists with id {id}."
            ));
        }

        if (status == PhysicalServerOperationStatus.OperatingSystemNotFound)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Operating system not found.",
                Code: ErrorCodes.BadRequest,
                Details: $"No operating system exists with id {request.OSId}."
            ));
        }

        if (status == PhysicalServerOperationStatus.DuplicateHostname)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "A server with this hostname already exists.",
                Code: ErrorCodes.Conflict,
                Details: $"The hostname '{request.Hostname}' is already assigned to another physical server."
            ));
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a physical server.
    /// </summary>
    /// <param name="id">The physical server identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The physical server was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete physical servers.</response>
    /// <response code="404">No physical server exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.PhysicalServers.Delete)]
    [SwaggerOperation(
        Summary = "Delete a physical server",
        Description = "Deletes an existing physical server by its identifier."
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
        var deleted = await _physicalServerService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Physical server not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No physical server exists with id {id}."
            ));
        }

        return NoContent();
    }
}
