using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.SoftwareVersions;
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
public class SoftwareVersionsController : ControllerBase
{

    private readonly ISoftwareVersionService _softwareVersionService;

    public SoftwareVersionsController(ISoftwareVersionService softwareVersionService)
    {
        _softwareVersionService = softwareVersionService;
    }

    [HttpPost]
    public async Task<ActionResult<SoftwareVersionResponse>> Create(CreateSoftwareVersionRequest request)
    {
        var softwareVersion = await _softwareVersionService.CreateAsync(
            request.SoftwareId,
            request.OsId,
            request.VersionNumber,
            request.InstallationDetails,
            request.Notes);

        if (softwareVersion is null)
            return BadRequest($"Software {request.SoftwareId} does not exist.");

        return CreatedAtAction(
            nameof(GetAll),
            new { id = softwareVersion.Id },
            softwareVersion.ToResponse());
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<SoftwareVersionResponse>>> GetAll()
    {
        var softwareVersions = await _softwareVersionService.GetAllAsync();
        return Ok(softwareVersions.Select(s => s.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SoftwareVersionResponse>> GetById(int id)
    {
        var softwareVersion = await _softwareVersionService.GetByIdAsync(id);
        if (softwareVersion is null)
            return NotFound();
        return Ok(softwareVersion.ToResponse());
    }

    [HttpGet("/api/v{version:apiVersion}/softwares/{softwareId:int}/[controller]")]
    public async Task<ActionResult<IEnumerable<SoftwareVersionResponse>>> GetAll(int? softwareId)
    {
        var softwareVersions = softwareId.HasValue
            ? await _softwareVersionService.GetAllBySoftwareIdAsync(softwareId.Value)
            : await _softwareVersionService.GetAllAsync();

        var response = softwareVersions.Select(i => i.ToResponse());

        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSoftwareVersionRequest request)
    {
        var updated = await _softwareVersionService.UpdateAsync(
            id,
            request.OsId,
            request.VersionNumber,
            request.InstallationDetails,
            request.Notes);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _softwareVersionService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

}
