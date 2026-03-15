using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Personnel;
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
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelService _personnelService;

    public PersonnelController(IPersonnelService personnelService)
    {
        _personnelService = personnelService;
    }

    [HttpPost]
    public async Task<ActionResult<PersonnelResponse>> Create(CreatePersonnelRequest request)
    {
        var result = await _personnelService.CreateAsync(
            request.FirstName,
            request.LastName,
            request.Function,
            request.Email);

        if (result.Status == PersonnelOperationStatus.DuplicateEmail)
            return BadRequest("A personnel with this email already exists");

        var personnel = result.Personnel!;

        return CreatedAtAction(
            nameof(GetById),
            new { id = personnel.Id },
            personnel.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonnelResponse>>> GetAll()
    {
        var personnels = await _personnelService.GetAllAsync();
        return Ok(personnels.Select(p => p.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PersonnelResponse>> GetById(int id)
    {
        var personnel = await _personnelService.GetByIdAsync(id);
        if (personnel is null)
            return NotFound();
        return Ok(personnel.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePersonnelRequest request)
    {
        var status = await _personnelService.UpdateAsync(
            id,
            request.FirstName,
            request.LastName,
            request.Function,
            request.Email);

        if (status == PersonnelOperationStatus.NotFound)
            return NotFound();

        if (status == PersonnelOperationStatus.DuplicateEmail)
            return BadRequest("A personnel with this email already exists");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _personnelService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
