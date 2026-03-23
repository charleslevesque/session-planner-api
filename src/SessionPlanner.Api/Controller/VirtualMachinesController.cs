using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.VirtualMachines;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.VirtualMachines;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Virtual Machines")]
public class VirtualMachinesController : ControllerBase
{
    private readonly IVirtualMachineService _virtualMachineService;

    public VirtualMachinesController(IVirtualMachineService virtualMachineService)
    {
        _virtualMachineService = virtualMachineService;
    }

    /// <summary>
    /// Creates a virtual machine.
    /// </summary>
    /// <param name="request">The physical server details, including it's host name, the number of cpu cores, the amount of ram (gb), the amount of storage (gb), 
    /// the access type, optional notes and the operating system id.</param>
    /// <returns>The newly created virtual machine.</returns>
    /// <response code="201">The virtual machine was created successfully.</response>
    /// <response code="400">The request is invalid, the operating system does not exist, or the host server does not exist.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create virtual machines.</response>
    [HttpPost]
    [HasPermission(Permissions.VirtualMachines.Create)]
    [SwaggerOperation(
        Summary = "Create a virtual machine",
        Description = "Creates a new virtual machine and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateVirtualMachineRequest), typeof(CreateVirtualMachineRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(VirtualMachineResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(VirtualMachineResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VirtualMachineResponse>> Create(CreateVirtualMachineRequest request)
    {
        var result = await _virtualMachineService.CreateAsync(
            request.Quantity,
            request.CpuCores,
            request.RamGb,
            request.StorageGb,
            request.AccessType,
            request.Notes,
            request.OSId,
            request.HostServerId);

        if (result.Status == VirtualMachineOperationStatus.OperatingSystemNotFound)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Operating system not found.",
                Code: ErrorCodes.BadRequest,
                Details: $"No operating system exists with id {request.OSId}."
            ));
        }

        if (result.Status == VirtualMachineOperationStatus.HostServerNotFound)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Host server not found.",
                Code: ErrorCodes.BadRequest,
                Details: $"No host server exists with id {request.HostServerId}."
            ));
        }

        var vm = result.VirtualMachine!;

        return CreatedAtAction(
            nameof(GetById),
            new { id = vm.Id },
            vm.ToResponse());
    }

    /// <summary>
    /// Retrieves all virtual machines.
    /// </summary>
    /// <returns>A list of virtual machines.</returns>
    /// <response code="200">The virtual machines were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read virtual machines.</response>
    [HttpGet]
    [HasPermission(Permissions.VirtualMachines.Read)]
    [SwaggerOperation(
        Summary = "Get all virtual machines",
        Description = "Returns all virtual machines."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(VirtualMachineListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<VirtualMachineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<VirtualMachineResponse>>> GetAll()
    {
        var vms = await _virtualMachineService.GetAllAsync();
        return Ok(vms.Select(v => v.ToResponse()));
    }

    /// <summary>
    /// Retrieves a virtual machine by identifier.
    /// </summary>
    /// <param name="id">The virtual machine identifier.</param>
    /// <returns>The matching virtual machine.</returns>
    /// <response code="200">The virtual machine was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read virtual machines.</response>
    /// <response code="404">No virtual machine exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.VirtualMachines.Read)]
    [SwaggerOperation(
        Summary = "Get a virtual machine by id",
        Description = "Returns a single virtual machine by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(VirtualMachineResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(VirtualMachineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VirtualMachineResponse>> GetById(int id)
    {
        var vm = await _virtualMachineService.GetByIdAsync(id);
        if (vm is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Virtual machine not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No virtual machine exists with id {id}."
            ));
        }
        return Ok(vm.ToResponse());
    }

    /// <summary>
    /// Updates an existing virtual machine.
    /// </summary>
    /// <param name="id">The virtual machine identifier.</param>
    /// <param name="request">The updated virtual machine data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The virtual machine was updated successfully.</response>
    /// <response code="400">The request is invalid, the operating system does not exist, or the host server does not exist.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update virtual machines.</response>
    /// <response code="404">No virtual machine exists with the supplied identifier.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.VirtualMachines.Update)]
    [SwaggerOperation(
        Summary = "Update a virtual machine",
        Description = "Updates an existing virtual machine by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdateVirtualMachineRequest), typeof(UpdateVirtualMachineRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateVirtualMachineRequest request)
    {
        var status = await _virtualMachineService.UpdateAsync(
            id,
            request.Quantity,
            request.CpuCores,
            request.RamGb,
            request.StorageGb,
            request.AccessType,
            request.Notes,
            request.OSId,
            request.HostServerId);

        if (status == VirtualMachineOperationStatus.NotFound)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Virtual machine not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No virtual machine exists with id {id}."
            ));
        }

        if (status == VirtualMachineOperationStatus.OperatingSystemNotFound)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Operating system not found.",
                Code: ErrorCodes.BadRequest,
                Details: $"No operating system exists with id {request.OSId}."
            ));
        }

        if (status == VirtualMachineOperationStatus.HostServerNotFound)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "Host server not found.",
                Code: ErrorCodes.BadRequest,
                Details: $"No host server exists with id {request.HostServerId}."
            ));
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a virtual machine.
    /// </summary>
    /// <param name="id">The virtual machine identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The virtual machine was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete virtual machines.</response>
    /// <response code="404">No virtual machine exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.VirtualMachines.Delete)]
    [SwaggerOperation(
        Summary = "Delete a virtual machine",
        Description = "Deletes an existing virtual machine by its identifier."
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
        var deleted = await _virtualMachineService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Virtual machine not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No virtual machine exists with id {id}."
            ));
        }

        return NoContent();
    }
}
