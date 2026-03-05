using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.Courses;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CoursesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<CourseResponse>> Create(CreateCourseRequest request)
    {
        var course = new Course
        {
            Code = request.Code,
            Name = request.Name
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = course.Id },
            course.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseResponse>>> GetAll()
    {
        var courses = await _db.Courses.ToListAsync();
        return Ok(courses.Select(c => c.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseResponse>> GetById(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course is null)
            return NotFound();
        return Ok(course.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCourseRequest request)
    {
        var course = await _db.Courses.FindAsync(id);

        if (course is null)
            return NotFound();

        course.Code = request.Code;
        course.Name = request.Name;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses.FindAsync(id);

        if (course is null)
            return NotFound();

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
