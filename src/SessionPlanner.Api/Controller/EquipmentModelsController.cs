using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.EquipmentModels;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class EquipmentModelsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EquipmentModelsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<EquipmentModelResponse>> Create(CreateEquipmentModelRequest request)
    {
        var equipment = new EquipmentModel
        {
            Name = request.Name,
            Quantity = request.Quantity,
            DefaultAccessories = request.DefaultAccessories,
            Notes = request.Notes
        };

        _db.EquipmentModels.Add(equipment);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = equipment.Id },
            equipment.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EquipmentModelResponse>>> GetAll()
    {
        var equipments = await _db.EquipmentModels.ToListAsync();
        return Ok(equipments.Select(e => e.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EquipmentModelResponse>> GetById(int id)
    {
        var equipment = await _db.EquipmentModels.FindAsync(id);
        if (equipment is null)
            return NotFound();
        return Ok(equipment.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEquipmentModelRequest request)
    {
        var equipment = await _db.EquipmentModels.FindAsync(id);

        if (equipment is null)
            return NotFound();

        equipment.Name = request.Name;
        equipment.Quantity = request.Quantity;
        equipment.DefaultAccessories = request.DefaultAccessories;
        equipment.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var equipment = await _db.EquipmentModels.FindAsync(id);

        if (equipment is null)
            return NotFound();

        _db.EquipmentModels.Remove(equipment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
