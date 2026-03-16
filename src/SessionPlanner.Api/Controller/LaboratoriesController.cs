using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Dtos.Workstations;
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
public class LaboratoriesController : ControllerBase
{
    private readonly ILaboratoryService _laboratoryService;

    public LaboratoriesController(ILaboratoryService laboratoryService)
    {
        _laboratoryService = laboratoryService;
    }

    // GET avec filtres (building, capacity)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LaboratoryResponse>>> GetAll(
        [FromQuery] string? building = null,
        [FromQuery] int? minCapacity = null,
        [FromQuery] int? maxCapacity = null)
    {
        var labs = await _laboratoryService.GetAllAsync(building, minCapacity, maxCapacity);
        return Ok(labs.Select(l => l.ToResponse()));
    }

    // GET par ID - Détail avec OS disponibles
    [HttpGet("{id}")]
    public async Task<ActionResult<LaboratoryResponse>> GetById(int id)
    {
        var lab = await _laboratoryService.GetByIdAsync(id);

        if (lab is null)
            return NotFound();

        return Ok(lab.ToResponse());
    }

    // POST - Création
    [HttpPost]
    public async Task<ActionResult<LaboratoryResponse>> Create(CreateLaboratoryRequest request)
    {
        var lab = await _laboratoryService.CreateAsync(
            request.Name,
            request.Building,
            request.NumberOfPCs,
            request.SeatingCapacity);

        return CreatedAtAction(nameof(GetById), new { id = lab.Id }, lab.ToResponse());
    }

    // PUT - Modification
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateLaboratoryRequest request)
    {
        var updated = await _laboratoryService.UpdateAsync(
            id,
            request.Name,
            request.Building,
            request.NumberOfPCs,
            request.SeatingCapacity);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // DELETE - Suppression
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _laboratoryService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    // POST /api/v1/Laboratories/{id}/workstations - Ajouter OS avec nombre de postes
    [HttpPost("{id}/workstations")]
    public async Task<ActionResult<WorkstationResponse>> AddWorkstation(int id, [FromBody] AddWorkstationRequest request)
    {
        var result = await _laboratoryService.AddWorkstationAsync(id, request.Name, request.OSId);

        if (result.Status == AddWorkstationStatus.LaboratoryNotFound)
            return NotFound();

        if (result.Status == AddWorkstationStatus.OperatingSystemNotFound)
            return BadRequest("Operating system not found");

        return CreatedAtAction(nameof(GetById), new { id }, result.Workstation!.ToResponse());
    }

    // DELETE /api/v1/Laboratories/{id}/workstations/{osId} - Retirer OS
    [HttpDelete("{id}/workstations/{workstationId}")]
    public async Task<IActionResult> RemoveWorkstation(int id, int workstationId)
    {
        var removed = await _laboratoryService.RemoveWorkstationAsync(id, workstationId);
        if (!removed)
            return NotFound();

        return NoContent();
    }
}
