using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.LaboratorySoftwares;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Interfaces;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class LaboratorySoftwaresController : ControllerBase
{
    private readonly ILaboratorySoftwareService _service;

    public LaboratorySoftwaresController(ILaboratorySoftwareService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LaboratorySoftwareResponse>>> GetAll(
        [FromQuery] int? laboratoryId = null)
    {
        var entries = laboratoryId.HasValue
            ? await _service.GetByLaboratoryAsync(laboratoryId.Value)
            : await _service.GetAllAsync();

        return Ok(entries.Select(e => e.ToResponse()));
    }

    [HttpPut("{laboratoryId}/{softwareId}")]
    public async Task<ActionResult<LaboratorySoftwareResponse>> Upsert(
        int laboratoryId,
        int softwareId,
        [FromBody] UpsertLaboratorySoftwareRequest request)
    {
        var result = await _service.UpsertAsync(laboratoryId, softwareId, request.Status);

        if (result is null)
            return NotFound();

        return Ok(result.ToResponse());
    }

    [HttpDelete("{laboratoryId}/{softwareId}")]
    public async Task<IActionResult> Delete(int laboratoryId, int softwareId)
    {
        var deleted = await _service.DeleteAsync(laboratoryId, softwareId);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
