using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Course> CreateAsync(string code, string? name)
    {
        var course = new Course
        {
            Code = code,
            Name = name
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return course;
    }

    public async Task<List<Course>> GetAllAsync()
    {
        return await _db.Courses.ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _db.Courses.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(int id, string code, string? name)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course is null)
            return false;

        course.Code = code;
        course.Name = name;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course is null)
            return false;

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();

        return true;
    }
}