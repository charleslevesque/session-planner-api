using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Courses;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpPost]
    public async Task<ActionResult<CourseResponse>> Create(CreateCourseRequest request)
    {
        var course = await _courseService.CreateAsync(request.Code, request.Name);

        return CreatedAtAction(
            nameof(GetById),
            new { id = course.Id },
            course.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseResponse>>> GetAll()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses.Select(c => c.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseResponse>> GetById(int id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course is null)
            return NotFound();
        return Ok(course.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCourseRequest request)
    {
        var updated = await _courseService.UpdateAsync(id, request.Code, request.Name);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _courseService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
