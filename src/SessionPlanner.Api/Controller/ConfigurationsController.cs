using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Configurations;
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
public class ConfigurationsController : ControllerBase
{
    private readonly IConfigurationService _configurationService;

    public ConfigurationsController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    [HttpPost]
    public async Task<ActionResult<ConfigurationResponse>> Create(CreateConfigurationRequest request)
    {
        var configuration = await _configurationService.CreateAsync(request.Title, request.Notes);

        return CreatedAtAction(
            nameof(GetById),
            new { id = configuration.Id },
            configuration.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConfigurationResponse>>> GetAll()
    {
        var configurations = await _configurationService.GetAllAsync();
        return Ok(configurations.Select(c => c.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConfigurationResponse>> GetById(int id)
    {
        var configuration = await _configurationService.GetByIdAsync(id);
        if (configuration is null)
            return NotFound();
        return Ok(configuration.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateConfigurationRequest request)
    {
        var updated = await _configurationService.UpdateAsync(id, request.Title, request.Notes);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _configurationService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
