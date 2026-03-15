using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.PhysicalServers;
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
public class PhysicalServersController : ControllerBase
{
    private readonly IPhysicalServerService _physicalServerService;

    public PhysicalServersController(IPhysicalServerService physicalServerService)
    {
        _physicalServerService = physicalServerService;
    }

    [HttpPost]
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
            return BadRequest("Operating system not found");

        if (result.Status == PhysicalServerOperationStatus.DuplicateHostname)
            return BadRequest("A server with this hostname already exists");

        var server = result.Server!;

        return CreatedAtAction(
            nameof(GetById),
            new { id = server.Id },
            server.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PhysicalServerResponse>>> GetAll()
    {
        var servers = await _physicalServerService.GetAllAsync();
        return Ok(servers.Select(s => s.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PhysicalServerResponse>> GetById(int id)
    {
        var server = await _physicalServerService.GetByIdAsync(id);
        if (server is null)
            return NotFound();
        return Ok(server.ToResponse());
    }

    [HttpPut("{id}")]
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
            return NotFound();

        if (status == PhysicalServerOperationStatus.OperatingSystemNotFound)
            return BadRequest("Operating system not found");

        if (status == PhysicalServerOperationStatus.DuplicateHostname)
            return BadRequest("A server with this hostname already exists");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _physicalServerService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
