using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Courses;
using SessionPlanner.Api.Dtos.CourseResources;
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

    /// <summary>
    /// Retrieves all resources associated with a course, grouped by type.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <returns>Resources grouped by type.</returns>
    /// <response code="200">The resources were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to read courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpGet("{courseId}/resources")]
    [HasPermission(Permissions.Courses.Read)]
    [SwaggerOperation(
        Summary = "Get all resources for a course",
        Description = "Returns all resources associated with a course, grouped by type."
    )]
    [ProducesResponseType(typeof(CourseResourcesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResourcesResponse>> GetResources(int courseId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var saas = await _courseService.GetCourseSaaSProductsAsync(courseId);
        var softwares = await _courseService.GetCourseSoftwaresAsync(courseId);
        var configs = await _courseService.GetCourseConfigurationsAsync(courseId);
        var vms = await _courseService.GetCourseVirtualMachinesAsync(courseId);
        var servers = await _courseService.GetCoursePhysicalServersAsync(courseId);
        var equipment = await _courseService.GetCourseEquipmentModelsAsync(courseId);
        var softwareVersionIds = await _courseService.GetCourseSoftwareVersionIdsAsync(courseId);

        var response = new CourseResourcesResponse(
            SaaS: saas.Select(x => x.ToCourseSaaSResponse()).ToList(),
            Softwares: softwares.Select(x => x.ToCourseSoftwareResponse()).ToList(),
            Configurations: configs.Select(x => x.ToCourseConfigurationResponse()).ToList(),
            VirtualMachines: vms.Select(x => x.ToCourseVmResponse()).ToList(),
            PhysicalServers: servers.Select(x => x.ToCourseServerResponse()).ToList(),
            Equipment: equipment.Select(x => x.ToCourseEquipmentResponse()).ToList(),
            SoftwareVersionIds: softwareVersionIds
        );

        return Ok(response);
    }

    /// <summary>
    /// Retrieves SaaS products associated with a course.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <returns>A list of SaaS products.</returns>
    /// <response code="200">The SaaS products were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to read courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpGet("{courseId}/saas")]
    [HasPermission(Permissions.Courses.Read)]
    [SwaggerOperation(
        Summary = "Get SaaS products for a course",
        Description = "Returns all SaaS products associated with a course."
    )]
    [ProducesResponseType(typeof(IEnumerable<CourseSaaSResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CourseSaaSResponse>>> GetSaaS(int courseId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var items = await _courseService.GetCourseSaaSProductsAsync(courseId);
        return Ok(items.Select(x => x.ToCourseSaaSResponse()));
    }

    /// <summary>
    /// Retrieves softwares associated with a course.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <returns>A list of softwares.</returns>
    /// <response code="200">The softwares were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to read courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpGet("{courseId}/softwares")]
    [HasPermission(Permissions.Courses.Read)]
    [SwaggerOperation(
        Summary = "Get softwares for a course",
        Description = "Returns all softwares associated with a course."
    )]
    [ProducesResponseType(typeof(IEnumerable<CourseSoftwareResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CourseSoftwareResponse>>> GetSoftwares(int courseId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var items = await _courseService.GetCourseSoftwaresAsync(courseId);
        return Ok(items.Select(x => x.ToCourseSoftwareResponse()));
    }

    /// <summary>
    /// Retrieves configurations associated with a course.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <returns>A list of configurations.</returns>
    /// <response code="200">The configurations were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to read courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpGet("{courseId}/configurations")]
    [HasPermission(Permissions.Courses.Read)]
    [SwaggerOperation(
        Summary = "Get configurations for a course",
        Description = "Returns all configurations associated with a course."
    )]
    [ProducesResponseType(typeof(IEnumerable<CourseConfigurationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CourseConfigurationResponse>>> GetConfigurations(int courseId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var items = await _courseService.GetCourseConfigurationsAsync(courseId);
        return Ok(items.Select(x => x.ToCourseConfigurationResponse()));
    }

    /// <summary>
    /// Retrieves virtual machines associated with a course.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <returns>A list of virtual machines.</returns>
    /// <response code="200">The virtual machines were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to read courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpGet("{courseId}/vms")]
    [HasPermission(Permissions.Courses.Read)]
    [SwaggerOperation(
        Summary = "Get virtual machines for a course",
        Description = "Returns all virtual machines associated with a course."
    )]
    [ProducesResponseType(typeof(IEnumerable<CourseVmResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CourseVmResponse>>> GetVirtualMachines(int courseId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var items = await _courseService.GetCourseVirtualMachinesAsync(courseId);
        return Ok(items.Select(x => x.ToCourseVmResponse()));
    }

    /// <summary>
    /// Retrieves physical servers associated with a course.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <returns>A list of physical servers.</returns>
    /// <response code="200">The physical servers were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to read courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpGet("{courseId}/servers")]
    [HasPermission(Permissions.Courses.Read)]
    [SwaggerOperation(
        Summary = "Get physical servers for a course",
        Description = "Returns all physical servers associated with a course."
    )]
    [ProducesResponseType(typeof(IEnumerable<CourseServerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CourseServerResponse>>> GetPhysicalServers(int courseId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var items = await _courseService.GetCoursePhysicalServersAsync(courseId);
        return Ok(items.Select(x => x.ToCourseServerResponse()));
    }

    /// <summary>
    /// Retrieves equipment associated with a course.
    /// </summary>
    /// <param name="courseId">The course identifier.</param>
    /// <returns>A list of equipment.</returns>
    /// <response code="200">The equipment was retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have permission to read courses.</response>
    /// <response code="404">No course exists with the supplied identifier.</response>
    [HttpGet("{courseId}/equipment")]
    [HasPermission(Permissions.Courses.Read)]
    [SwaggerOperation(
        Summary = "Get equipment for a course",
        Description = "Returns all equipment associated with a course."
    )]
    [ProducesResponseType(typeof(IEnumerable<CourseEquipmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CourseEquipmentResponse>>> GetEquipment(int courseId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var items = await _courseService.GetCourseEquipmentModelsAsync(courseId);
        return Ok(items.Select(x => x.ToCourseEquipmentResponse()));
    }

    // ── Association / Dissociation endpoints ──

    /// <summary>Associates a SaaS product with a course.</summary>
    [HttpPost("{courseId}/saas/{saasProductId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Associate a SaaS product with a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssociateSaaS(int courseId, int saasProductId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var result = await _courseService.AssociateSaaSProductAsync(courseId, saasProductId);
        if (result is null) return ResourceNotFound("SaaS product", saasProductId);
        if (result == false) return AlreadyAssociated();
        return NoContent();
    }

    /// <summary>Dissociates a SaaS product from a course.</summary>
    [HttpDelete("{courseId}/saas/{saasProductId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Dissociate a SaaS product from a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DissociateSaaS(int courseId, int saasProductId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var removed = await _courseService.DissociateSaaSProductAsync(courseId, saasProductId);
        if (!removed) return AssociationNotFound();
        return NoContent();
    }

    /// <summary>Associates a software with a course.</summary>
    [HttpPost("{courseId}/softwares/{softwareId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Associate a software with a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssociateSoftware(int courseId, int softwareId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var result = await _courseService.AssociateSoftwareAsync(courseId, softwareId);
        if (result is null) return ResourceNotFound("Software", softwareId);
        if (result == false) return AlreadyAssociated();
        return NoContent();
    }

    /// <summary>Dissociates a software from a course.</summary>
    [HttpDelete("{courseId}/softwares/{softwareId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Dissociate a software from a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DissociateSoftware(int courseId, int softwareId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var removed = await _courseService.DissociateSoftwareAsync(courseId, softwareId);
        if (!removed) return AssociationNotFound();
        return NoContent();
    }

    /// <summary>Associates a configuration with a course.</summary>
    [HttpPost("{courseId}/configurations/{configurationId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Associate a configuration with a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssociateConfiguration(int courseId, int configurationId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var result = await _courseService.AssociateConfigurationAsync(courseId, configurationId);
        if (result is null) return ResourceNotFound("Configuration", configurationId);
        if (result == false) return AlreadyAssociated();
        return NoContent();
    }

    /// <summary>Dissociates a configuration from a course.</summary>
    [HttpDelete("{courseId}/configurations/{configurationId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Dissociate a configuration from a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DissociateConfiguration(int courseId, int configurationId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var removed = await _courseService.DissociateConfigurationAsync(courseId, configurationId);
        if (!removed) return AssociationNotFound();
        return NoContent();
    }

    /// <summary>Associates a virtual machine with a course.</summary>
    [HttpPost("{courseId}/vms/{virtualMachineId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Associate a virtual machine with a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssociateVirtualMachine(int courseId, int virtualMachineId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var result = await _courseService.AssociateVirtualMachineAsync(courseId, virtualMachineId);
        if (result is null) return ResourceNotFound("Virtual machine", virtualMachineId);
        if (result == false) return AlreadyAssociated();
        return NoContent();
    }

    /// <summary>Dissociates a virtual machine from a course.</summary>
    [HttpDelete("{courseId}/vms/{virtualMachineId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Dissociate a virtual machine from a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DissociateVirtualMachine(int courseId, int virtualMachineId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var removed = await _courseService.DissociateVirtualMachineAsync(courseId, virtualMachineId);
        if (!removed) return AssociationNotFound();
        return NoContent();
    }

    /// <summary>Associates a physical server with a course.</summary>
    [HttpPost("{courseId}/servers/{physicalServerId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Associate a physical server with a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssociatePhysicalServer(int courseId, int physicalServerId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var result = await _courseService.AssociatePhysicalServerAsync(courseId, physicalServerId);
        if (result is null) return ResourceNotFound("Physical server", physicalServerId);
        if (result == false) return AlreadyAssociated();
        return NoContent();
    }

    /// <summary>Dissociates a physical server from a course.</summary>
    [HttpDelete("{courseId}/servers/{physicalServerId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Dissociate a physical server from a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DissociatePhysicalServer(int courseId, int physicalServerId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var removed = await _courseService.DissociatePhysicalServerAsync(courseId, physicalServerId);
        if (!removed) return AssociationNotFound();
        return NoContent();
    }

    /// <summary>Associates an equipment model with a course.</summary>
    [HttpPost("{courseId}/equipment/{equipmentModelId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Associate an equipment model with a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssociateEquipmentModel(int courseId, int equipmentModelId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var result = await _courseService.AssociateEquipmentModelAsync(courseId, equipmentModelId);
        if (result is null) return ResourceNotFound("Equipment model", equipmentModelId);
        if (result == false) return AlreadyAssociated();
        return NoContent();
    }

    /// <summary>Dissociates an equipment model from a course.</summary>
    [HttpDelete("{courseId}/equipment/{equipmentModelId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Dissociate an equipment model from a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DissociateEquipmentModel(int courseId, int equipmentModelId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var removed = await _courseService.DissociateEquipmentModelAsync(courseId, equipmentModelId);
        if (!removed) return AssociationNotFound();
        return NoContent();
    }

    /// <summary>Associates a software version with a course.</summary>
    [HttpPost("{courseId}/softwareversions/{softwareVersionId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Associate a software version with a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssociateSoftwareVersion(int courseId, int softwareVersionId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var result = await _courseService.AssociateSoftwareVersionAsync(courseId, softwareVersionId);
        if (result is null) return ResourceNotFound("Software version", softwareVersionId);
        if (result == false) return AlreadyAssociated();
        return NoContent();
    }

    /// <summary>Dissociates a software version from a course.</summary>
    [HttpDelete("{courseId}/softwareversions/{softwareVersionId}")]
    [HasPermission(Permissions.Courses.Update)]
    [SwaggerOperation(Summary = "Dissociate a software version from a course")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DissociateSoftwareVersion(int courseId, int softwareVersionId)
    {
        if (!await _courseService.ExistsAsync(courseId))
            return CourseNotFound(courseId);

        var removed = await _courseService.DissociateSoftwareVersionAsync(courseId, softwareVersionId);
        if (!removed) return AssociationNotFound();
        return NoContent();
    }

    // ── Helpers ──

    private NotFoundObjectResult CourseNotFound(int courseId)
    {
        return NotFound(new ApiErrorResponse(
            Error: "Course not found.",
            Code: ErrorCodes.NotFound,
            Details: $"No course exists with id {courseId}."
        ));
    }

    private NotFoundObjectResult ResourceNotFound(string resourceType, int resourceId)
    {
        return NotFound(new ApiErrorResponse(
            Error: $"{resourceType} not found.",
            Code: ErrorCodes.NotFound,
            Details: $"No {resourceType.ToLowerInvariant()} exists with id {resourceId}."
        ));
    }

    private ConflictObjectResult AlreadyAssociated()
    {
        return Conflict(new ApiErrorResponse(
            Error: "Already associated.",
            Code: ErrorCodes.Conflict,
            Details: "This resource is already associated with the course."
        ));
    }

    private NotFoundObjectResult AssociationNotFound()
    {
        return NotFound(new ApiErrorResponse(
            Error: "Association not found.",
            Code: ErrorCodes.NotFound,
            Details: "This resource is not associated with the course."
        ));
    }
}
