using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SessionsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Session session)
    {
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll),
            new { id = session.Id },
            session);
    }

    [HttpGet]
    [HasPermission(Permissions.Sessions.Read)]
    public async Task<ActionResult<IEnumerable<Session>>> GetAll()
    {
        return await _db.Sessions.ToListAsync();
    }
}