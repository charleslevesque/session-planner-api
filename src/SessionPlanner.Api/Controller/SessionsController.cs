using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using System.Security.Claims;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Api.Dtos.Sessions;
using SessionPlanner.Api.Mappings;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    [HasPermission(Permissions.Sessions.Read)]
    public async Task<ActionResult<IEnumerable<SessionResponse>>> GetAll([FromQuery] bool? active)
    {
        var sessions = await _sessionService.GetAllAsync(active);
        return Ok(sessions.Select(s => s.ToResponse()));
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Sessions.Read)]
    public async Task<ActionResult<SessionResponse>> GetById(int id)
    {
        var session = await _sessionService.GetByIdAsync(id);
        if (session is null) return NotFound();
        return Ok(session.ToResponse());
    }

    [HttpPost]
    [HasPermission(Permissions.Sessions.Create)]
    public async Task<ActionResult<SessionResponse>> Create(CreateSessionRequest request)
    {
        if (request.EndDate <= request.StartDate)
            return BadRequest(new { error = "EndDate must be after StartDate." });

        int? userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : null;
        var session = await _sessionService.CreateAsync(request.Title, request.StartDate, request.EndDate, userId);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, session.ToResponse());
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Sessions.Update)]
    public async Task<ActionResult<SessionResponse>> Update(int id, UpdateSessionRequest request)
    {
        if (request.EndDate <= request.StartDate)
            return BadRequest(new { error = "EndDate must be after StartDate." });

        var session = await _sessionService.UpdateAsync(id, request.Title, request.StartDate, request.EndDate);
        if (session is null) return NotFound();
        return Ok(session.ToResponse());
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Sessions.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _sessionService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:int}/open")]
    [HasPermission(Permissions.Sessions.Update)]
    public async Task<ActionResult<SessionResponse>> Open(int id)
    {
        return await HandleTransition(() => _sessionService.OpenAsync(id));
    }

    [HttpPost("{id:int}/close")]
    [HasPermission(Permissions.Sessions.Update)]
    public async Task<ActionResult<SessionResponse>> Close(int id)
    {
        return await HandleTransition(() => _sessionService.CloseAsync(id));
    }

    [HttpPost("{id:int}/archive")]
    [HasPermission(Permissions.Sessions.Update)]
    public async Task<ActionResult<SessionResponse>> Archive(int id)
    {
        return await HandleTransition(() => _sessionService.ArchiveAsync(id));
    }

    private async Task<ActionResult<SessionResponse>> HandleTransition(Func<Task<Core.Entities.Session?>> transition)
    {
        try
        {
            var session = await transition();
            if (session is null) return NotFound();
            return Ok(session.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
