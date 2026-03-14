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

    public async Task<Session> CreateAsync(string title, DateTime startDate, DateTime endDate, int? createdByUserId = null)
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

    private async Task<Session?> TransitionAsync(int id, SessionStatus target)
    {
        var session = await _db.Sessions.FindAsync(id);
        if (session is null) return null;

        session.TransitionTo(target);
        await _db.SaveChangesAsync();
        return session;
    }
}
