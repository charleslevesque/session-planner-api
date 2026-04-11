using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface ISessionService
{
    Task<List<Session>> GetAllAsync(bool? activeOnly);
    Task<Session?> GetByIdAsync(int id);
    Task<Session> CreateAsync(string title, DateTime startDate, DateTime endDate, int? createdByUserId = null, IReadOnlyList<int>? courseIds = null, int? copyFromSessionId = null);
    Task<Session?> UpdateAsync(int id, string title, DateTime startDate, DateTime endDate);
    Task<bool> DeleteAsync(int id);
    Task<Session?> OpenAsync(int id);
    Task<Session?> CloseAsync(int id);
    Task<Session?> ArchiveAsync(int id);

    Task<List<Course>> GetSessionCoursesAsync(int sessionId);
    Task<List<Course>> ReplaceSessionCoursesAsync(int sessionId, IReadOnlyList<int> courseIds);
}
