using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
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
        var query = _db.Sessions.AsQueryable();

        if (activeOnly == true)
            query = query.Where(s => s.Status == SessionStatus.Open);

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    public async Task<Session?> GetByIdAsync(int id)
    {
        return await _db.Sessions.FindAsync(id);
    }

    public async Task<Session> CreateAsync(string title, DateTime startDate, DateTime endDate)
    {
        var session = new Session
        {
            Title = title,
            Status = SessionStatus.Draft,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();

        return session;
    }

    public async Task<Session?> UpdateAsync(int id, string title, DateTime startDate, DateTime endDate)
    {
        var session = await _db.Sessions.FindAsync(id);
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
        return await TransitionAsync(id, SessionStatus.Draft, SessionStatus.Open,
            "Only draft sessions can be opened.");
    }

    public async Task<Session?> CloseAsync(int id)
    {
        return await TransitionAsync(id, SessionStatus.Open, SessionStatus.Closed,
            "Only open sessions can be closed.");
    }

    public async Task<Session?> ArchiveAsync(int id)
    {
        return await TransitionAsync(id, SessionStatus.Closed, SessionStatus.Archived,
            "Only closed sessions can be archived.");
    }

    private async Task<Session?> TransitionAsync(int id, SessionStatus from, SessionStatus to, string errorMessage)
    {
        var session = await _db.Sessions.FindAsync(id);
        if (session is null) return null;

        if (session.Status != from)
            throw new InvalidOperationException(errorMessage);

        session.Status = to;
        await _db.SaveChangesAsync();
        return session;
    }
}
