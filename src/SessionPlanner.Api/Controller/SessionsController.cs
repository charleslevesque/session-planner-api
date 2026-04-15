using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Api.Dtos.Sessions;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.Sessions;
using SessionPlanner.Api.OpenApi.Examples.Common;
using SessionPlanner.Infrastructure.Data;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly AppDbContext _db;

    public SessionsController(ISessionService sessionService, AppDbContext db)
    {
        _sessionService = sessionService;
        _db = db;
    }

    /// <summary>
    /// Retrieves all sessions.
    /// </summary>
    /// <param name="active">An optional filter indicating whether to return only active or inactive sessions.</param>
    /// <returns>A list of sessions.</returns>
    /// <response code="200">The sessions were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read sessions.</response>
    [HttpGet]
    [HasPermission(Permissions.Sessions.Read)]
    [SwaggerOperation(
        Summary = "Get all sessions",
        Description = "Returns all sessions, optionally filtered by active status."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SessionListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<SessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SessionResponse>>> GetAll([FromQuery] bool? active)
    {
        var sessions = await _sessionService.GetAllAsync(active);
        return Ok(sessions.Select(s => s.ToResponse()));
    }

    /// <summary>
    /// Retrieves a session by identifier.
    /// </summary>
    /// <param name="id">The session identifier.</param>
    /// <returns>The matching session.</returns>
    /// <response code="200">The session was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read sessions.</response>
    /// <response code="404">No session exists with the supplied identifier.</response>
    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Sessions.Read)]
    [SwaggerOperation(
        Summary = "Get a session by id",
        Description = "Returns a single session by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SessionResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionResponse>> GetById(int id)
    {
        var session = await _sessionService.GetByIdAsync(id);
        if (session is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Session not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No session exists with id {id}."
            ));
        }
        return Ok(session.ToResponse());
    }

    /// <summary>
    /// Creates a session.
    /// </summary>
    /// <param name="request">The session details, including it's title, the start date, the end date, and the current user's identifier.</param>
    /// <returns>The newly created session.</returns>
    /// <response code="201">The session was created successfully.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create sessions.</response>
    [HttpPost]
    [HasPermission(Permissions.Sessions.Create)]
    [SwaggerOperation(
        Summary = "Create a session",
        Description = "Creates a new session and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateSessionRequest), typeof(CreateSessionRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(SessionResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(InvalidSessionDatesExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SessionResponse>> Create(CreateSessionRequest request)
    {
        if (request.EndDate <= request.StartDate)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "EndDate must be after StartDate.",
                Code: ErrorCodes.InvalidSessionDates
            ));
        }

        int? userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : null;
        var session = await _sessionService.CreateAsync(
            request.Title, request.StartDate, request.EndDate, userId,
            request.CourseIds, request.CopyFromSessionId);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, session.ToResponse());
    }

    /// <summary>
    /// Updates an existing session.
    /// </summary>
    /// <param name="id">The session identifier.</param>
    /// <param name="request">The updated session data.</param>
    /// <returns>The updated session.</returns>
    /// <response code="200">The session was updated successfully.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update sessions.</response>
    /// <response code="404">No session exists with the supplied identifier.</response>
    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Sessions.Update)]
    [SwaggerOperation(
        Summary = "Update a session",
        Description = "Updates an existing session by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdateSessionRequest), typeof(UpdateSessionRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SessionResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(InvalidSessionDatesExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionResponse>> Update(int id, UpdateSessionRequest request)
    {
         if (request.EndDate <= request.StartDate)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "EndDate must be after StartDate.",
                Code: ErrorCodes.InvalidSessionDates
            ));
        }

        var session = await _sessionService.UpdateAsync(id, request.Title, request.StartDate, request.EndDate);
        if (session is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Session not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No session exists with id {id}."
            ));
        }
        return Ok(session.ToResponse());
    }

    /// <summary>
    /// Deletes a session.
    /// </summary>
    /// <param name="id">The session identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The session was deleted successfully.</response>
    /// <response code="400">The session cannot be deleted in its current state.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete sessions.</response>
    /// <response code="404">No session exists with the supplied identifier.</response>
    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Sessions.Delete)]
    [SwaggerOperation(
        Summary = "Delete a session",
        Description = "Deletes an existing session by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(SessionDeleteNotAllowedExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _sessionService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Session not found.",
                    Code: ErrorCodes.NotFound,
                    Details: $"No session exists with id {id}."
                ));
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorResponse(
                Error: ex.Message,
                Code: ErrorCodes.SessionDeleteNotAllowed
            ));
        }
    }

    /// <summary>
    /// Returns the courses associated with a session.
    /// </summary>
    [HttpGet("{id:int}/courses")]
    [HasPermission(Permissions.Sessions.Read)]
    [SwaggerOperation(
        Summary = "Get session courses",
        Description = "Returns all courses associated with a session."
    )]
    [ProducesResponseType(typeof(IEnumerable<SessionCourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<SessionCourseResponse>>> GetCourses(int id)
    {
        var session = await _sessionService.GetByIdAsync(id);
        if (session is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Session not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No session exists with id {id}."
            ));
        }

        var courses = await _sessionService.GetSessionCoursesAsync(id);
        return Ok(courses.Select(c => c.ToCourseResponse()));
    }

    /// <summary>
    /// Replaces the courses associated with a session.
    /// </summary>
    [HttpPut("{id:int}/courses")]
    [HasPermission(Permissions.Sessions.Update)]
    [SwaggerOperation(
        Summary = "Replace session courses",
        Description = "Replaces the full list of courses associated with a session. Only allowed for Draft and Open sessions."
    )]
    [ProducesResponseType(typeof(IEnumerable<SessionCourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IEnumerable<SessionCourseResponse>>> ReplaceCourses(int id, UpdateSessionCoursesRequest request)
    {
        try
        {
            var courses = await _sessionService.ReplaceSessionCoursesAsync(id, request.CourseIds);
            return Ok(courses.Select(c => c.ToCourseResponse()));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Session not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No session exists with id {id}."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(
                Error: ex.Message,
                Code: ErrorCodes.SessionCoursesNotModifiable
            ));
        }
    }

    /// <summary>
    /// Opens a draft session.
    /// </summary>
    /// <remarks>
    /// Transitions a session from Draft to Open.
    /// </remarks>
    /// <param name="id">The session identifier.</param>
    /// <returns>The updated session.</returns>
    /// <response code="200">The session was opened successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update sessions.</response>
    /// <response code="404">No session exists with the supplied identifier.</response>
    /// <response code="409">The session cannot be transitioned from its current state.</response>
    [HttpPost("{id:int}/open")]
    [HasPermission(Permissions.Sessions.Update)]
    [SwaggerOperation(
        Summary = "Open a session",
        Description = "Transitions a session from Draft to Open."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SessionResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidSessionTransitionExample))]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionResponse>> Open(int id)
    {
        return await HandleTransition(() => _sessionService.OpenAsync(id));
    }

    /// <summary>
    /// Closes an open session.
    /// </summary>
    /// <remarks>
    /// Transitions a session from Open to Closed.
    /// </remarks>
    /// <param name="id">The session identifier.</param>
    /// <returns>The updated session.</returns>
    /// <response code="200">The session was closed successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update sessions.</response>
    /// <response code="404">No session exists with the supplied identifier.</response>
    /// <response code="409">The session cannot be transitioned from its current state.</response>
    [HttpPost("{id:int}/close")]
    [HasPermission(Permissions.Sessions.Update)]
    [SwaggerOperation(
        Summary = "Close a session",
        Description = "Transitions a session from Open to Closed."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SessionResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidSessionTransitionExample))]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionResponse>> Close(int id)
    {
        return await HandleTransition(() => _sessionService.CloseAsync(id));
    }

    /// <summary>
    /// Archives a closed session.
    /// </summary>
    /// <remarks>
    /// Transitions a session from Closed to Archived.
    /// </remarks>
    /// <param name="id">The session identifier.</param>
    /// <returns>The updated session.</returns>
    /// <response code="200">The session was archived successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update sessions.</response>
    /// <response code="404">No session exists with the supplied identifier.</response>
    /// <response code="409">The session cannot be transitioned from its current state.</response>
    [HttpPost("{id:int}/archive")]
    [HasPermission(Permissions.Sessions.Update)]
    [SwaggerOperation(
        Summary = "Archive a session",
        Description = "Transitions a session from Closed to Archived."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SessionResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(InvalidSessionTransitionExample))]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionResponse>> Archive(int id)
    {
        return await HandleTransition(() => _sessionService.ArchiveAsync(id));
    }

    // GET /api/v1/sessions/{id}/export
    [HttpGet("{id:int}/export")]
    [HasPermission(Permissions.TeachingNeeds.Read)]
    [SwaggerOperation(
        Summary = "Export installation matrix as CSV",
        Description = "Downloads the full installation matrix for a session. Contains every software from the catalogue with its status in every lab, enriched with session-specific demand info (courses, professors)."
    )]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportCsv(int id)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session is null)
            return NotFound(new ApiErrorResponse("Session not found.", ErrorCodes.NotFound));

        // 1. Full software catalogue with versions
        var allSoftwares = await _db.Softwares
            .Include(s => s.SoftwareVersions).ThenInclude(sv => sv.OS)
            .OrderBy(s => s.Name)
            .ToListAsync();

        // 2. All labs
        var labs = await _db.Laboratories
            .OrderBy(l => l.Building).ThenBy(l => l.Name)
            .ToListAsync();

        // 3. Full LaboratorySoftware matrix (status per lab×software)
        var allLabSw = await _db.LaboratorySoftwares.ToListAsync();
        var statusLookup = allLabSw.ToDictionary(ls => (ls.LaboratoryId, ls.SoftwareId), ls => ls.Status);

        // 4. OS lookup
        var osMap = await _db.OperatingSystems.ToDictionaryAsync(o => o.Id, o => o.Name);

        // 5. Session demand context: which software was requested, by whom, for which course
        var needs = await _db.TeachingNeeds
            .Include(n => n.Personnel)
            .Include(n => n.Course)
            .Include(n => n.Items)
            .Where(n => n.SessionId == id)
            .Where(n => n.Status != Core.Enums.NeedStatus.Draft)
            .ToListAsync();

        var demandBySoftwareId = new Dictionary<int, SessionDemandInfo>();
        foreach (var need in needs)
        {
            foreach (var item in need.Items.Where(i => i.SoftwareId.HasValue))
            {
                var swId = item.SoftwareId!.Value;
                if (!demandBySoftwareId.TryGetValue(swId, out var info))
                {
                    info = new SessionDemandInfo(new HashSet<string>(), new HashSet<string>(), new HashSet<string>());
                    demandBySoftwareId[swId] = info;
                }
                if (need.Course?.Code is { } code) info.Courses.Add(code);
                if (need.Personnel is { } p) info.Professors.Add($"{p.FirstName} {p.LastName}");
                if (!string.IsNullOrWhiteSpace(item.Notes)) info.RequestNotes.Add(item.Notes);
            }
        }

        // 6. Build pivoted CSV: one row per software, one column per lab
        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        // Header row: software info columns + one column per lab (showing "LabName (Building) [NbPCs postes]")
        var headerCols = new List<string>
        {
            "Logiciel", "Version(s)", "OS", "Commande d'installation", "Notes",
            "Demandé session", "Cours", "Professeur(s)", "Notes demande"
        };
        foreach (var lab in labs)
            headerCols.Add($"{lab.Name} ({lab.Building}) [{lab.NumberOfPCs}p]");

        sb.AppendLine(string.Join(";", headerCols.Select(Esc)));

        // One row per software
        foreach (var sw in allSoftwares)
        {
            var versions = sw.SoftwareVersions
                .Select(v => v.VersionNumber)
                .Distinct()
                .ToList();
            var osList = sw.SoftwareVersions
                .Select(v => v.OsId)
                .Distinct()
                .Select(oid => osMap.TryGetValue(oid, out var name) ? name : $"OS#{oid}")
                .ToList();
            var versionNotes = sw.SoftwareVersions
                .Where(v => !string.IsNullOrWhiteSpace(v.Notes))
                .Select(v => v.Notes!)
                .Distinct()
                .ToList();

            var hasDemand = demandBySoftwareId.TryGetValue(sw.Id, out var demand);

            var row = new List<string>
            {
                Esc(sw.Name),
                Esc(string.Join(", ", versions)),
                Esc(string.Join(", ", osList)),
                Esc(sw.InstallCommand ?? ""),
                Esc(string.Join(" | ", versionNotes)),
                hasDemand ? "Oui" : "",
                hasDemand ? Esc(string.Join(", ", demand!.Courses)) : "",
                hasDemand ? Esc(string.Join(", ", demand!.Professors)) : "",
                hasDemand ? Esc(string.Join(" | ", demand!.RequestNotes)) : ""
            };

            foreach (var lab in labs)
            {
                var status = statusLookup.TryGetValue((lab.Id, sw.Id), out var st) ? st : "";
                row.Add(Esc(status));
            }

            sb.AppendLine(string.Join(";", row));
        }

        var safeTitle = session.Title.Replace(" ", "_").Replace("/", "-");
        var fileName = $"installations_{safeTitle}_{DateTime.UtcNow:yyyyMMdd}.csv";
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private record SessionDemandInfo(
        HashSet<string> Courses, HashSet<string> Professors, HashSet<string> RequestNotes);

    private static string Esc(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private async Task<ActionResult<SessionResponse>> HandleTransition(Func<Task<Core.Entities.Session?>> transition)
    {
        try
        {
            var session = await transition();
            if (session is null)
            {
                return NotFound(new ApiErrorResponse(
                    Error: "Session not found.",
                    Code: ErrorCodes.NotFound
                ));
            }

            return Ok(session.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiErrorResponse(
                Error: ex.Message,
                Code: ErrorCodes.InvalidSessionTransition
            ));
        }
    }
}
