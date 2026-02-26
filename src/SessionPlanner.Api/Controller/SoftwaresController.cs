using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.Softwares;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SoftwaresController : ControllerBase
{
    private readonly AppDbContext _db;

    public SoftwaresController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<SoftwareResponse>> Create(CreateSoftwareRequest request)
    {

        var software = new Software
        {

            Name = request.Name

        };

        _db.Softwares.Add(software);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAll),
            new { id = software.Id },
            software.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SoftwareResponse>>> GetAll()
    {
        var softwares = await _db.Softwares.Include(s => s.SoftwareVersions).ToListAsync();
        return Ok(softwares.Select(s => s.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SoftwareResponse>> GetById(int id)
    {
        var software = await _db.Softwares
            .Include(s => s.SoftwareVersions)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (software is null)
            return NotFound();
        return Ok(software.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSoftwareRequest request)
    {
        var software = await _db.Softwares.FindAsync(id);

        if (software is null)
            return NotFound();

        software.Name = request.Name;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var software = await _db.Softwares.FindAsync(id);

        if (software is null)
            return NotFound();

        _db.Softwares.Remove(software);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}