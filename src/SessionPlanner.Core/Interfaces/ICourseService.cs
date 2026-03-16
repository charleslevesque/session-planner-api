using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public interface ICourseService
{
    Task<Course> CreateAsync(string code, string? name);
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, string code, string? name);
    Task<bool> DeleteAsync(int id);
}