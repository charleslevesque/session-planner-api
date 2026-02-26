using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.OperatingSystems;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class OperatingSystemsController : ControllerBase
{
    private readonly AppDbContext _db;

    public OperatingSystemsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OSResponse>>> GetAll()
    {
        var osList = await _db.OperatingSystems.Include(s => s.SoftwareVersions).ToListAsync();
        return Ok(osList.Select(os => os.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OSResponse>> GetById(int id)
    {
        var os = await _db.OperatingSystems.FindAsync(id);
        if (os is null)
            return NotFound();
        return Ok(os.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<OSResponse>> Create(CreateOSRequest request)
    {
        var os = request.toEntity();
        _db.OperatingSystems.Add(os);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = os.Id }, os.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateOSRequest request)
    {
        var os = await _db.OperatingSystems.FindAsync(id);
        if (os is null)
            return NotFound();
        request.Apply(os);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var os = await _db.OperatingSystems.FindAsync(id);
        if (os is null)
            return NotFound();
        _db.OperatingSystems.Remove(os);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
