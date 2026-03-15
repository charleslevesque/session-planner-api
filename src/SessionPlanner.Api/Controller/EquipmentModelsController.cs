using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.EquipmentModels;
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
public class EquipmentModelsController : ControllerBase
{
    private readonly IEquipmentModelService _equipmentModelService;

    public EquipmentModelsController(IEquipmentModelService equipmentModelService)
    {
        _equipmentModelService = equipmentModelService;
    }

    [HttpPost]
    public async Task<ActionResult<EquipmentModelResponse>> Create(CreateEquipmentModelRequest request)
    {
        var equipment = await _equipmentModelService.CreateAsync(
            request.Name,
            request.Quantity,
            request.DefaultAccessories,
            request.Notes);

        return CreatedAtAction(
            nameof(GetById),
            new { id = equipment.Id },
            equipment.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EquipmentModelResponse>>> GetAll()
    {
        var equipments = await _equipmentModelService.GetAllAsync();
        return Ok(equipments.Select(e => e.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EquipmentModelResponse>> GetById(int id)
    {
        var equipment = await _equipmentModelService.GetByIdAsync(id);
        if (equipment is null)
            return NotFound();
        return Ok(equipment.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEquipmentModelRequest request)
    {
        var updated = await _equipmentModelService.UpdateAsync(
            id,
            request.Name,
            request.Quantity,
            request.DefaultAccessories,
            request.Notes);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _equipmentModelService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
