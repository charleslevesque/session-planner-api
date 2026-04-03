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

    public async Task<int?> GetOrCreatePersonnelIdForUserAsync(int userId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user is null)
            return null;

        if (user.PersonnelId is not null)
            return user.PersonnelId;

        var rawUsername = (user.Username ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(rawUsername))
            return null;

        // Reuse existing Personnel by email when present to avoid duplicates.
        var existingPersonnel = await _db.Personnel
            .FirstOrDefaultAsync(p => p.Email == rawUsername);

        if (existingPersonnel is not null)
        {
            user.PersonnelId = existingPersonnel.Id;
            await _db.SaveChangesAsync();
            return existingPersonnel.Id;
        }

        var localPart = rawUsername.Split('@')[0];
        var nameParts = localPart
            .Split(new[] { '.', '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var firstName = nameParts.Length > 0 ? ToTitle(nameParts[0]) : "Teacher";
        var lastName = nameParts.Length > 1 ? ToTitle(nameParts[1]) : "User";

        var personnel = new Personnel
        {
            FirstName = firstName,
            LastName = lastName,
            Email = rawUsername,
            Function = PersonnelFunction.Professor,
        };

        _db.Personnel.Add(personnel);
        await _db.SaveChangesAsync();

        user.PersonnelId = personnel.Id;
        await _db.SaveChangesAsync();

        return personnel.Id;
    }

    private static string ToTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "User";
        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
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

    public async Task<List<TeachingNeed>> GetMyNeedsAsync(int personnelId, int? sessionId = null, int? courseId = null, IEnumerable<NeedStatus>? statuses = null)
    {
        var query = _db.TeachingNeeds
            .Include(n => n.Session)
            .Include(n => n.Personnel)
            .Include(n => n.Course)
            .Where(n => n.PersonnelId == personnelId);

        if (sessionId.HasValue)
            query = query.Where(n => n.SessionId == sessionId.Value);

        if (courseId.HasValue)
            query = query.Where(n => n.CourseId == courseId.Value);

        if (statuses is not null)
        {
            var statusList = statuses.ToList();
            if (statusList.Count > 0)
                query = query.Where(n => statusList.Contains(n.Status));
        }

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

    public async Task<TeachingNeed> CreateAsync(int sessionId, int personnelId, int courseId, string? notes,
        int? expectedStudents = null, bool? hasTechNeeds = null, bool? foundAllCourses = null,
        string? desiredModifications = null, bool? allowsUpdates = null, string? additionalComments = null)
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
            Notes = notes,
            ExpectedStudents = expectedStudents,
            HasTechNeeds = hasTechNeeds,
            FoundAllCourses = foundAllCourses,
            DesiredModifications = desiredModifications,
            AllowsUpdates = allowsUpdates,
            AdditionalComments = additionalComments
        };

        _db.TeachingNeeds.Add(need);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(sessionId, need.Id)
            ?? throw new InvalidOperationException("Failed to reload created teaching need.");
    }

    public async Task<TeachingNeed?> UpdateAsync(int sessionId, int id, int courseId, string? notes,
        int? expectedStudents = null, bool? hasTechNeeds = null, bool? foundAllCourses = null,
        string? desiredModifications = null, bool? allowsUpdates = null, string? additionalComments = null)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        if (need.Status != NeedStatus.Draft && need.Status != NeedStatus.Submitted && need.Status != NeedStatus.Rejected)
            throw new InvalidOperationException("Need can only be modified when in Draft, Submitted, or Rejected status.");

        need.CourseId = courseId;
        need.Notes = notes;
        need.ExpectedStudents = expectedStudents;
        need.HasTechNeeds = hasTechNeeds;
        need.FoundAllCourses = foundAllCourses;
        need.DesiredModifications = desiredModifications;
        need.AllowsUpdates = allowsUpdates;
        need.AdditionalComments = additionalComments;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(sessionId, id);
    }

    public async Task<bool> DeleteAsync(int sessionId, int id)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return false;

        if (need.Status == NeedStatus.Approved)
            throw new InvalidOperationException("Approved needs cannot be cancelled.");

        _db.TeachingNeeds.Remove(need);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<TeachingNeedItem?> AddItemAsync(int sessionId, int needId, string itemType, int? softwareId, int? softwareVersionId, int? osId, int? quantity, string? description, string? notes, string? detailsJson)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == needId);

        if (need is null) return null;

        if (need.Status != NeedStatus.Draft && need.Status != NeedStatus.Submitted)
            throw new InvalidOperationException("Items can only be added to Draft or Submitted needs.");

        var item = new TeachingNeedItem
        {
            TeachingNeedId = needId,
            ItemType = itemType,
            SoftwareId = softwareId,
            SoftwareVersionId = softwareVersionId,
            OSId = osId,
            Quantity = quantity,
            Description = description,
            Notes = notes,
            DetailsJson = detailsJson
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

        if (need.Status != NeedStatus.Draft && need.Status != NeedStatus.Submitted)
            throw new InvalidOperationException("Items can only be removed from Draft or Submitted needs.");

        var item = await _db.TeachingNeedItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TeachingNeedId == needId);

        if (item is null) return false;

        _db.TeachingNeedItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<TeachingNeed?> SubmitAsync(int sessionId, int id)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        EnsureStatus(need, NeedStatus.Draft, "Need can only be submitted from Draft status.");

        need.Status = NeedStatus.Submitted;
        need.SubmittedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(sessionId, id);
    }

    public async Task<TeachingNeed?> ReviewAsync(int sessionId, int id)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        EnsureStatus(need, NeedStatus.Submitted, "Need can only be moved to UnderReview from Submitted status.");

        need.Status = NeedStatus.UnderReview;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(sessionId, id);
    }

    public async Task<TeachingNeed?> ApproveAsync(int sessionId, int id, int? reviewedByUserId)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        EnsureStatus(need, NeedStatus.UnderReview, "Need can only be approved from UnderReview status.");

        need.Status = NeedStatus.Approved;
        need.ReviewedAt = DateTime.UtcNow;
        need.ReviewedByUserId = reviewedByUserId;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(sessionId, id);
    }

    public async Task<TeachingNeed?> RejectAsync(int sessionId, int id, string reason, int? reviewedByUserId)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        EnsureStatus(need, NeedStatus.UnderReview, "Need can only be rejected from UnderReview status.");

        need.Status = NeedStatus.Rejected;
        need.RejectionReason = reason;
        need.ReviewedAt = DateTime.UtcNow;
        need.ReviewedByUserId = reviewedByUserId;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(sessionId, id);
    }

    public async Task<TeachingNeed?> ReviseAsync(int sessionId, int id)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        EnsureStatus(need, NeedStatus.Rejected, "Need can only be revised from Rejected status.");

        need.Status = NeedStatus.Draft;
        need.RejectionReason = null;
        need.ReviewedAt = null;
        need.ReviewedByUserId = null;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(sessionId, id);
    }

    private static void EnsureStatus(TeachingNeed need, NeedStatus expectedStatus, string message)
    {
        if (need.Status != expectedStatus)
            throw new InvalidOperationException(message);
    }
}
