using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Dtos.Workstations;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class LaboratoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public LaboratoriesController(AppDbContext db)
    {
        _db = db;
    }

    // GET avec filtres (building, capacity)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LaboratoryResponse>>> GetAll(
        [FromQuery] string? building = null,
        [FromQuery] int? minCapacity = null,
        [FromQuery] int? maxCapacity = null)
    {
        var query = _db.Laboratories
            .Include(l => l.Workstations)
                .ThenInclude(w => w.OperatingSystem)
            .AsQueryable();

        if (!string.IsNullOrEmpty(building))
        {
            query = query.Where(l => l.Building == building);
        }

        if (minCapacity.HasValue)
        {
            query = query.Where(l => l.SeatingCapacity >= minCapacity.Value);
        }

        if (maxCapacity.HasValue)
        {
            query = query.Where(l => l.SeatingCapacity <= maxCapacity.Value);
        }

        var labs = await query.ToListAsync();
        return Ok(labs.Select(l => l.ToResponse()));
    }

    // GET par ID - Détail avec OS disponibles
    [HttpGet("{id}")]
    public async Task<ActionResult<LaboratoryResponse>> GetById(int id)
    {
        var lab = await _db.Laboratories
            .Include(l => l.Workstations)
                .ThenInclude(w => w.OperatingSystem)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lab is null)
            return NotFound();

        return Ok(lab.ToResponse());
    }

    // POST - Création
    [HttpPost]
    public async Task<ActionResult<LaboratoryResponse>> Create(CreateLaboratoryRequest request)
    {
        var lab = request.ToEntity();

        _db.Laboratories.Add(lab);
        await _db.SaveChangesAsync();

        await _db.Entry(lab).Collection(l => l.Workstations).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = lab.Id }, lab.ToResponse());
    }

    // PUT - Modification
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateLaboratoryRequest request)
    {
        var lab = await _db.Laboratories.FindAsync(id);

        if (lab is null)
            return NotFound();

        request.Apply(lab);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE - Suppression
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var lab = await _db.Laboratories.FindAsync(id);

        if (lab is null)
            return NotFound();

        _db.Laboratories.Remove(lab);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // POST /api/v1/Laboratories/{id}/workstations - Ajouter OS avec nombre de postes
    [HttpPost("{id}/workstations")]
    public async Task<ActionResult<WorkstationResponse>> AddWorkstation(int id, [FromBody] AddWorkstationRequest request)
    {
        var lab = await _db.Laboratories.FindAsync(id);
        if (lab is null)
            return NotFound();

        var os = await _db.OperatingSystems.FindAsync(request.OperatingSystemId);
        if (os is null)
            return BadRequest("Operating system not found");

        var existingWorkstation = await _db.Workstations
            .FirstOrDefaultAsync(w => w.LaboratoryId == id && w.OperatingSystemId == request.OperatingSystemId);

        if (existingWorkstation is not null)
        {
            existingWorkstation.Count += request.Count;
            await _db.SaveChangesAsync();
            await _db.Entry(existingWorkstation).Reference(w => w.OperatingSystem).LoadAsync();
            return Ok(existingWorkstation.ToResponse());
        }

        var workstation = new Workstation
        {
            LaboratoryId = id,
            OperatingSystemId = request.OperatingSystemId,
            Count = request.Count
        };

        _db.Workstations.Add(workstation);
        await _db.SaveChangesAsync();

        await _db.Entry(workstation).Reference(w => w.OperatingSystem).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = lab.Id }, workstation.ToResponse());
    }

    // DELETE /api/v1/Laboratories/{id}/workstations/{osId} - Retirer OS
    [HttpDelete("{id}/workstations/{osId}")]
    public async Task<IActionResult> RemoveWorkstation(int id, int osId)
    {
        var lab = await _db.Laboratories.FindAsync(id);
        if (lab is null)
            return NotFound();

        var workstation = await _db.Workstations
            .FirstOrDefaultAsync(w => w.LaboratoryId == id && w.OperatingSystemId == osId);

        if (workstation is null)
            return NotFound();

        _db.Workstations.Remove(workstation);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
