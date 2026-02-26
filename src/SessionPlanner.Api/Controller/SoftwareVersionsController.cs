using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.SoftwareVersions;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SoftwareVersionsController : ControllerBase
{

    private readonly AppDbContext _db;

    public SoftwareVersionsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<SoftwareVersionResponse>> Create(CreateSoftwareVersionRequest request)
    {

        var softwareExists = await _db.Softwares
        .AnyAsync(s => s.Id == request.SoftwareId);


        if (!softwareExists)
            return BadRequest($"Software {request.SoftwareId} does not exist.");

        var softwareVersion = new SoftwareVersion
        {

            SoftwareId = request.SoftwareId,
            VersionNumber = request.VersionNumber,
            OsId = request.OsId,
            InstallationDetails = request.InstallationDetails,
            Notes = request.Notes,

        };

        _db.SoftwareVersions.Add(softwareVersion);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAll),
            new { id = softwareVersion.Id },
            softwareVersion.ToResponse());
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<SoftwareVersionResponse>>> GetAll()
    {
        var softwareVersions = await _db.SoftwareVersions
        .ToListAsync();
        return Ok(softwareVersions.Select(s => s.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SoftwareVersionResponse>> GetById(int id)
    {
        var softwareVersion = await _db.SoftwareVersions.FindAsync(id);
        if (softwareVersion is null)
            return NotFound();
        return Ok(softwareVersion.ToResponse());
    }

    [HttpGet("/api/v{version:apiVersion}/softwares/{softwareId:int}/[controller]")]
    public async Task<ActionResult<IEnumerable<SoftwareVersionResponse>>> GetAll(int? softwareId)
    {
        var query = _db.SoftwareVersions
            .AsQueryable();

        if (softwareId.HasValue)
            query = query.Where(x => x.SoftwareId == softwareId);

        var softwareVersions = await query.ToListAsync();

        var response = softwareVersions.Select(i => i.ToResponse());

        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSoftwareVersionRequest request)
    {
        var softwareVersion = await _db.SoftwareVersions.FindAsync(id);

        if (softwareVersion is null)
            return NotFound();

        softwareVersion.OsId = request.OsId;
        softwareVersion.VersionNumber = request.VersionNumber;
        softwareVersion.InstallationDetails = request.InstallationDetails;
        softwareVersion.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var softwareVersion = await _db.SoftwareVersions.FindAsync(id);

        if (softwareVersion is null)
            return NotFound();

        _db.SoftwareVersions.Remove(softwareVersion);
        await _db.SaveChangesAsync();

        return NoContent();
    }

}
