using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.Configurations;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ConfigurationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ConfigurationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<ConfigurationResponse>> Create(CreateConfigurationRequest request)
    {
        var configuration = new Configuration
        {
            Title = request.Title,
            Notes = request.Notes
        };

        _db.Configurations.Add(configuration);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = configuration.Id },
            configuration.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConfigurationResponse>>> GetAll()
    {
        var configurations = await _db.Configurations.ToListAsync();
        return Ok(configurations.Select(c => c.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConfigurationResponse>> GetById(int id)
    {
        var configuration = await _db.Configurations.FindAsync(id);
        if (configuration is null)
            return NotFound();
        return Ok(configuration.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateConfigurationRequest request)
    {
        var configuration = await _db.Configurations.FindAsync(id);

        if (configuration is null)
            return NotFound();

        configuration.Title = request.Title;
        configuration.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var configuration = await _db.Configurations.FindAsync(id);

        if (configuration is null)
            return NotFound();

        _db.Configurations.Remove(configuration);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
