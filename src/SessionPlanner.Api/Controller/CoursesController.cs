using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Courses;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.Courses;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// Creates a course.
    /// </summary>
    /// <remarks>
    /// Creates a new course using the supplied course code and name.
    /// </remarks>
    /// <param name="request">The course details.</param>
    /// <returns>The newly created course.</returns>
    /// <response code="201">The course was created successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to create courses.</response>
    [HttpPost]
    [HasPermission(Permissions.Courses.Create)]
    [SwaggerOperation(
        Summary = "Create a course",
        Description = "Creates a new course and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateCourseRequest), typeof(CreateCourseRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(CourseResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CourseResponse>> Create(CreateCourseRequest request)
    {
        var course = await _courseService.CreateAsync(request.Code, request.Name);

        return CreatedAtAction(
            nameof(GetById),
            new { id = course.Id },
            course.ToResponse());
    }

    /// <summary>
    /// Retrieves all courses.
    /// </summary>
    /// <remarks>
    /// Returns all courses.
    /// </remarks>
    /// <returns>A list of courses.</returns>
    /// <response code="200">The courses were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to read courses.</response>
    [HttpGet]
    [HasPermission(Permissions.Courses.Read)]
    [SwaggerOperation(
        Summary = "Get all courses",
        Description = "Returns all courses."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CourseListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CourseResponse>>> GetAll()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses.Select(c => c.ToResponse()));
    }

    /// <summary>
    /// Retrieves a course by identifier.
    /// </summary>
    /// <remarks>
    /// Returns a single course by its identifier.
    /// </remarks>
    /// <param name="id">The course identifier.</param>
    /// <returns>The matching course.</returns>
    /// <response code="200">The course was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to read courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.Courses.Read)]
    [SwaggerOperation(
        Summary = "Get a course by id",
        Description = "Returns a single course by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CourseResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResponse>> GetById(int id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Course not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No course exists with id {id}."
            ));
        }
        return Ok(course.ToResponse());
    }

    /// <summary>
    /// Updates an existing course.
    /// </summary>
    /// <remarks>
    /// Updates the code and name of an existing course.
    /// </remarks>
    /// <param name="id">The course identifier.</param>
    /// <param name="request">The updated course data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The course was updated successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to update courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(
        Summary = "Update a course",
        Description = "Updates an existing course by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdateCourseRequest), typeof(UpdateCourseRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateCourseRequest request)
    {
        var updated = await _courseService.UpdateAsync(id, request.Code, request.Name);

        if (!updated)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Course not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No course exists with id {id}."
            ));
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a course.
    /// </summary>
    /// <remarks>
    /// Deletes an existing course by its identifier.
    /// </remarks>
    /// <param name="id">The course identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The course was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to delete courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Courses.Delete)]
    [SwaggerOperation(
        Summary = "Delete a course",
        Description = "Deletes an existing course by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _courseService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Course not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No course exists with id {id}."
            ));
        }

        return NoContent();
    }
}
