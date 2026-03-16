using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Enums;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class TeachingNeedService : ITeachingNeedService
{
    private readonly AppDbContext _db;

    public TeachingNeedService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int?> GetPersonnelIdForUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user?.PersonnelId;
    }

    public async Task<List<TeachingNeed>> GetAllBySessionAsync(int sessionId, int? filterByPersonnelId = null)
    {
        var query = _db.TeachingNeeds
            .Include(n => n.Personnel)
            .Include(n => n.Course)
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Items).ThenInclude(i => i.SoftwareVersion)
            .Include(n => n.Items).ThenInclude(i => i.OS)
            .Where(n => n.SessionId == sessionId);

        if (filterByPersonnelId.HasValue)
            query = query.Where(n => n.PersonnelId == filterByPersonnelId.Value);

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<TeachingNeed?> GetByIdAsync(int sessionId, int id)
    {
        return await _db.TeachingNeeds
            .Include(n => n.Personnel)
            .Include(n => n.Course)
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Items).ThenInclude(i => i.SoftwareVersion)
            .Include(n => n.Items).ThenInclude(i => i.OS)
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);
    }

    public async Task<TeachingNeed> CreateAsync(int sessionId, int personnelId, int courseId, string? notes)
    {
        var session = await _db.Sessions.FindAsync(sessionId)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Status != SessionStatus.Open)
            throw new InvalidOperationException("Cannot create a need: the session is not open.");

        var need = new TeachingNeed
        {
            SessionId = sessionId,
            PersonnelId = personnelId,
            CourseId = courseId,
            Notes = notes
        };

        _db.TeachingNeeds.Add(need);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(sessionId, need.Id)
            ?? throw new InvalidOperationException("Failed to reload created teaching need.");
    }

    public async Task<TeachingNeed?> UpdateAsync(int sessionId, int id, int courseId, string? notes)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        if (need.Status == NeedStatus.Approved)
            throw new InvalidOperationException("Cannot modify an approved need.");

        if (need.Status != NeedStatus.Draft && need.Status != NeedStatus.Rejected)
            throw new InvalidOperationException("Need can only be modified when in Draft or Rejected status.");

        need.CourseId = courseId;
        need.Notes = notes;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(sessionId, id);
    }

    public async Task<bool> DeleteAsync(int sessionId, int id)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return false;

        if (need.Status != NeedStatus.Draft)
            throw new InvalidOperationException("Only Draft needs can be deleted.");

        _db.TeachingNeeds.Remove(need);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<TeachingNeedItem?> AddItemAsync(int sessionId, int needId, int? softwareId, int? softwareVersionId, int? osId, int? quantity, string? notes)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == needId);

        if (need is null) return null;

        if (need.Status != NeedStatus.Draft)
            throw new InvalidOperationException("Items can only be added to Draft needs.");

        var item = new TeachingNeedItem
        {
            TeachingNeedId = needId,
            SoftwareId = softwareId,
            SoftwareVersionId = softwareVersionId,
            OSId = osId,
            Quantity = quantity,
            Notes = notes
        };

        _db.TeachingNeedItems.Add(item);
        await _db.SaveChangesAsync();

        return await _db.TeachingNeedItems
            .Include(i => i.Software)
            .Include(i => i.SoftwareVersion)
            .Include(i => i.OS)
            .FirstOrDefaultAsync(i => i.Id == item.Id);
    }

    public async Task<bool> RemoveItemAsync(int sessionId, int needId, int itemId)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == needId);

        if (need is null) return false;

        if (need.Status != NeedStatus.Draft)
            throw new InvalidOperationException("Items can only be removed from Draft needs.");

        var item = await _db.TeachingNeedItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TeachingNeedId == needId);

        if (item is null) return false;

        _db.TeachingNeedItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}
