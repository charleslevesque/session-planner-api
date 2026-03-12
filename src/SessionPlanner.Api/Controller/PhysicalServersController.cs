using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.PhysicalServers;
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
public class PhysicalServersController : ControllerBase
{
    private readonly AppDbContext _db;

    public PhysicalServersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<PhysicalServerResponse>> Create(CreatePhysicalServerRequest request)
    {
        var os = await _db.OperatingSystems.FindAsync(request.OSId);
        if (os is null)
            return BadRequest("Operating system not found");

        var existingServer = await _db.PhysicalServers
            .FirstOrDefaultAsync(s => s.Hostname == request.Hostname);
        if (existingServer is not null)
            return BadRequest("A server with this hostname already exists");

        var server = new PhysicalServer
        {
            Hostname = request.Hostname,
            CpuCores = request.CpuCores,
            RamGb = request.RamGb,
            StorageGb = request.StorageGb,
            AccessType = request.AccessType,
            Notes = request.Notes,
            OSId = request.OSId
        };

        _db.PhysicalServers.Add(server);
        await _db.SaveChangesAsync();

        await _db.Entry(server).Reference(s => s.OS).LoadAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = server.Id },
            server.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PhysicalServerResponse>>> GetAll()
    {
        var servers = await _db.PhysicalServers
            .Include(s => s.OS)
            .ToListAsync();
        return Ok(servers.Select(s => s.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PhysicalServerResponse>> GetById(int id)
    {
        var server = await _db.PhysicalServers
            .Include(s => s.OS)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (server is null)
            return NotFound();
        return Ok(server.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePhysicalServerRequest request)
    {
        var server = await _db.PhysicalServers.FindAsync(id);

        if (server is null)
            return NotFound();

        var os = await _db.OperatingSystems.FindAsync(request.OSId);
        if (os is null)
            return BadRequest("Operating system not found");

        var existingServer = await _db.PhysicalServers
            .FirstOrDefaultAsync(s => s.Hostname == request.Hostname && s.Id != id);
        if (existingServer is not null)
            return BadRequest("A server with this hostname already exists");

        server.Hostname = request.Hostname;
        server.CpuCores = request.CpuCores;
        server.RamGb = request.RamGb;
        server.StorageGb = request.StorageGb;
        server.AccessType = request.AccessType;
        server.Notes = request.Notes;
        server.OSId = request.OSId;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var server = await _db.PhysicalServers.FindAsync(id);

        if (server is null)
            return NotFound();

        _db.PhysicalServers.Remove(server);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
