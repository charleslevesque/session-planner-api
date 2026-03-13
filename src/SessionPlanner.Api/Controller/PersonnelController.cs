using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.Personnel;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using PersonnelEntity = SessionPlanner.Core.Entities.Personnel;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class PersonnelController : ControllerBase
{
    private readonly AppDbContext _db;

    public PersonnelController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<PersonnelResponse>> Create(CreatePersonnelRequest request)
    {
        var existingPersonnel = await _db.Personnel
            .FirstOrDefaultAsync(p => p.Email == request.Email);
        if (existingPersonnel is not null)
            return BadRequest("A personnel with this email already exists");

        var personnel = new PersonnelEntity
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Function = request.Function,
            Email = request.Email
        };

        _db.Personnel.Add(personnel);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = personnel.Id },
            personnel.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonnelResponse>>> GetAll()
    {
        var personnels = await _db.Personnel.ToListAsync();
        return Ok(personnels.Select(p => p.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PersonnelResponse>> GetById(int id)
    {
        var personnel = await _db.Personnel.FindAsync(id);
        if (personnel is null)
            return NotFound();
        return Ok(personnel.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePersonnelRequest request)
    {
        var personnel = await _db.Personnel.FindAsync(id);

        if (personnel is null)
            return NotFound();

        var existingPersonnel = await _db.Personnel
            .FirstOrDefaultAsync(p => p.Email == request.Email && p.Id != id);
        if (existingPersonnel is not null)
            return BadRequest("A personnel with this email already exists");

        personnel.FirstName = request.FirstName;
        personnel.LastName = request.LastName;
        personnel.Function = request.Function;
        personnel.Email = request.Email;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var personnel = await _db.Personnel.FindAsync(id);

        if (personnel is null)
            return NotFound();

        _db.Personnel.Remove(personnel);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
