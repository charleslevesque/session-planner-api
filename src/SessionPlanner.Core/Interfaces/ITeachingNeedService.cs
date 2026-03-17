using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface ITeachingNeedService
{
    /// <summary>Returns all needs for a session. If filterByPersonnelId is set, only that personnel's needs are returned.</summary>
    Task<List<TeachingNeed>> GetAllBySessionAsync(int sessionId, int? filterByPersonnelId = null);

    Task<TeachingNeed?> GetByIdAsync(int sessionId, int id);

    /// <summary>Creates a need. Throws InvalidOperationException if session is not Open.</summary>
    Task<TeachingNeed> CreateAsync(int sessionId, int personnelId, int courseId, string? notes);

    /// <summary>Updates a need. Returns null if not found. Throws InvalidOperationException if status is Approved or not Draft/Rejected.</summary>
    Task<TeachingNeed?> UpdateAsync(int sessionId, int id, int courseId, string? notes);

    /// <summary>Deletes a need. Returns false if not found. Throws InvalidOperationException if status is not Draft.</summary>
    Task<bool> DeleteAsync(int sessionId, int id);

    /// <summary>Adds an item to a need. Returns null if need not found. Throws InvalidOperationException if status is not Draft.</summary>
    Task<TeachingNeedItem?> AddItemAsync(int sessionId, int needId, int? softwareId, int? softwareVersionId, int? osId, int? quantity, string? notes);

    /// <summary>Removes an item from a need. Returns false if need or item not found. Throws InvalidOperationException if status is not Draft.</summary>
    Task<bool> RemoveItemAsync(int sessionId, int needId, int itemId);

    /// <summary>Transitions Draft -> Submitted. Returns null if not found.</summary>
    Task<TeachingNeed?> SubmitAsync(int sessionId, int id);

    /// <summary>Transitions Submitted -> UnderReview. Returns null if not found.</summary>
    Task<TeachingNeed?> ReviewAsync(int sessionId, int id);

    /// <summary>Transitions UnderReview -> Approved. Returns null if not found.</summary>
    Task<TeachingNeed?> ApproveAsync(int sessionId, int id, int? reviewedByUserId);

    /// <summary>Transitions UnderReview -> Rejected with a reason. Returns null if not found.</summary>
    Task<TeachingNeed?> RejectAsync(int sessionId, int id, string reason, int? reviewedByUserId);

    /// <summary>Transitions Rejected -> Draft. Returns null if not found.</summary>
    Task<TeachingNeed?> ReviseAsync(int sessionId, int id);

    /// <summary>Returns the PersonnelId linked to the given userId, or null if none.</summary>
    Task<int?> GetPersonnelIdForUserAsync(int userId);

    /// <summary>
    /// Returns linked PersonnelId for a user, creating/linking a Personnel record when missing.
    /// Intended for teacher flows where account-to-personnel link may be absent.
    /// </summary>
    Task<int?> GetOrCreatePersonnelIdForUserAsync(int userId);
}
