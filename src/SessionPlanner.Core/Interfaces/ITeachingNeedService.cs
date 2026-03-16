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

    /// <summary>Returns the PersonnelId linked to the given userId, or null if none.</summary>
    Task<int?> GetPersonnelIdForUserAsync(int userId);
}
