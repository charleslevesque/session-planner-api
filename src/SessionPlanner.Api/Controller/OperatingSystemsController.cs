using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Api.Dtos.OperatingSystems;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Interfaces;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class OperatingSystemsController : ControllerBase
{
    private readonly IOperatingSystemService _operatingSystemService;

    public OperatingSystemsController(IOperatingSystemService operatingSystemService)
    {
        _operatingSystemService = operatingSystemService;
    }

    [HttpGet]
    [HasPermission(Permissions.OperatingSystems.Read)]
    public async Task<ActionResult<IEnumerable<OSResponse>>> GetAll()
    {
        var osList = await _operatingSystemService.GetAllAsync();
        return Ok(osList.Select(os => os.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OSResponse>> GetById(int id)
    {
        var os = await _operatingSystemService.GetByIdAsync(id);
        if (os is null)
            return NotFound();
        return Ok(os.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<OSResponse>> Create(CreateOSRequest request)
    {
        var os = await _operatingSystemService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetById), new { id = os.Id }, os.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateOSRequest request)
    {
        var updated = await _operatingSystemService.UpdateAsync(id, request.Name);
        if (!updated)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _operatingSystemService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
