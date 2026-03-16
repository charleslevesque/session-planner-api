using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Softwares;
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
public class SoftwaresController : ControllerBase
{
    private readonly ISoftwareService _softwareService;

    public SoftwaresController(ISoftwareService softwareService)
    {
        _softwareService = softwareService;
    }

    [HttpPost]
    public async Task<ActionResult<SoftwareResponse>> Create(CreateSoftwareRequest request)
    {
        var software = await _softwareService.CreateAsync(request.Name);

        return CreatedAtAction(
            nameof(GetAll),
            new { id = software.Id },
            software.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SoftwareResponse>>> GetAll()
    {
        var softwares = await _softwareService.GetAllAsync();
        return Ok(softwares.Select(s => s.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SoftwareResponse>> GetById(int id)
    {
        var software = await _softwareService.GetByIdAsync(id);
        if (software is null)
            return NotFound();
        return Ok(software.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSoftwareRequest request)
    {
        var updated = await _softwareService.UpdateAsync(id, request.Name);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _softwareService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}