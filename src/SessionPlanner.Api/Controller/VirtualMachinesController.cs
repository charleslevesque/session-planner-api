using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.VirtualMachines;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class VirtualMachinesController : ControllerBase
{
    private readonly IVirtualMachineService _virtualMachineService;

    public VirtualMachinesController(IVirtualMachineService virtualMachineService)
    {
        _virtualMachineService = virtualMachineService;
    }

    [HttpPost]
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
            return BadRequest("Operating system not found");

        if (result.Status == VirtualMachineOperationStatus.HostServerNotFound)
            return BadRequest("Host server not found");

        var vm = result.VirtualMachine!;

        return CreatedAtAction(
            nameof(GetById),
            new { id = vm.Id },
            vm.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VirtualMachineResponse>>> GetAll()
    {
        var vms = await _virtualMachineService.GetAllAsync();
        return Ok(vms.Select(v => v.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VirtualMachineResponse>> GetById(int id)
    {
        var vm = await _virtualMachineService.GetByIdAsync(id);
        if (vm is null)
            return NotFound();
        return Ok(vm.ToResponse());
    }

    [HttpPut("{id}")]
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
            return NotFound();

        if (status == VirtualMachineOperationStatus.OperatingSystemNotFound)
            return BadRequest("Operating system not found");

        if (status == VirtualMachineOperationStatus.HostServerNotFound)
            return BadRequest("Host server not found");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _virtualMachineService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
