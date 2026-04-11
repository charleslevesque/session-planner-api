using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Enums;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly AppDbContext _db;

    public SessionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Session>> GetAllAsync(bool? activeOnly)
    {
        var query = _db.Sessions
            .Include(s => s.SessionCourses)
            .AsQueryable();

        if (activeOnly == true)
            query = query.Where(s => s.Status == SessionStatus.Open);

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    public async Task<Session?> GetByIdAsync(int id)
    {
        return await _db.Sessions
            .Include(s => s.SessionCourses)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Session> CreateAsync(string title, DateTime startDate, DateTime endDate,
        int? createdByUserId = null, IReadOnlyList<int>? courseIds = null, int? copyFromSessionId = null)
    {
        var session = new Session
        {
            Title = title,
            StartDate = startDate,
            EndDate = endDate,
            CreatedByUserId = createdByUserId
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        var idsToAssociate = new List<int>();

        if (copyFromSessionId.HasValue)
        {
            var sourceCourseIds = await _db.SessionCourses
                .Where(sc => sc.SessionId == copyFromSessionId.Value)
                .Select(sc => sc.CourseId)
                .ToListAsync();
            idsToAssociate.AddRange(sourceCourseIds);
        }

        if (courseIds is { Count: > 0 })
        {
            idsToAssociate.AddRange(courseIds);
        }

        if (idsToAssociate.Count > 0)
        {
            var validIds = await _db.Courses
                .Where(c => idsToAssociate.Distinct().Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var cid in validIds)
            {
                _db.SessionCourses.Add(new SessionCourse { SessionId = session.Id, CourseId = cid });
            }

            await _db.SaveChangesAsync();
        }

        await _db.Entry(session).Collection(s => s.SessionCourses).LoadAsync();
        return session;
    }

    public async Task<Session?> UpdateAsync(int id, string title, DateTime startDate, DateTime endDate)
    {
        var session = await _db.Sessions
            .Include(s => s.SessionCourses)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (session is null) return null;

        session.Title = title;
        session.StartDate = startDate;
        session.EndDate = endDate;

        await _db.SaveChangesAsync();
        return session;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var session = await _db.Sessions.FindAsync(id);
        if (session is null) return false;

        if (session.Status == SessionStatus.Open)
            throw new InvalidOperationException("Cannot delete an open session. Close it first.");

        _db.Sessions.Remove(session);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Session?> OpenAsync(int id)
    {
        return await TransitionAsync(id, SessionStatus.Open);
    }

    public async Task<Session?> CloseAsync(int id)
    {
        return await TransitionAsync(id, SessionStatus.Closed);
    }

    public async Task<Session?> ArchiveAsync(int id)
    {
        return await TransitionAsync(id, SessionStatus.Archived);
    }

    public async Task<List<Course>> GetSessionCoursesAsync(int sessionId)
    {
        return await _db.SessionCourses
            .Where(sc => sc.SessionId == sessionId)
            .Select(sc => sc.Course)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<List<Course>> ReplaceSessionCoursesAsync(int sessionId, IReadOnlyList<int> courseIds)
    {
        var session = await _db.Sessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException("Session not found.");

        if (session.Status != SessionStatus.Draft && session.Status != SessionStatus.Open)
            throw new InvalidOperationException("Cannot modify courses: session must be Draft or Open.");

        var existing = await _db.SessionCourses
            .Where(sc => sc.SessionId == sessionId)
            .ToListAsync();

        _db.SessionCourses.RemoveRange(existing);

        if (courseIds.Count > 0)
        {
            var validIds = await _db.Courses
                .Where(c => courseIds.Distinct().Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var cid in validIds)
            {
                _db.SessionCourses.Add(new SessionCourse { SessionId = sessionId, CourseId = cid });
            }
        }

        await _db.SaveChangesAsync();

        return await GetSessionCoursesAsync(sessionId);
    }

    private async Task<Session?> TransitionAsync(int id, SessionStatus target)
    {
        var session = await _db.Sessions
            .Include(s => s.SessionCourses)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (session is null) return null;

        session.TransitionTo(target);
        await _db.SaveChangesAsync();
        return session;
    }
}
