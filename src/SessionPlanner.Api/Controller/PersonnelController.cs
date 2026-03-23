using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Personnel;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.Personnel;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Personnel")]
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelService _personnelService;

    public PersonnelController(IPersonnelService personnelService)
    {
        _personnelService = personnelService;
    }

    /// <summary>
    /// Creates a personnel profile.
    /// </summary>
    /// <param name="request">The personnel details, including their first name, their last name, their function and their email.</param>
    /// <returns>The newly created personnel profile.</returns>
    /// <response code="201">The personnel profile was created successfully.</response>
    /// <response code="400">A personnel profile with the supplied email already exists.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create personnel profiles.</response>
    [HttpPost]
    [HasPermission(Permissions.Personnels.Create)]
    [SwaggerOperation(
        Summary = "Create a personnel profile",
        Description = "Creates a new personnel profile and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreatePersonnelRequest), typeof(CreatePersonnelRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(PersonnelResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DuplicatePersonnelEmailExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(PersonnelResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PersonnelResponse>> Create(CreatePersonnelRequest request)
    {
        var result = await _personnelService.CreateAsync(
            request.FirstName,
            request.LastName,
            request.Function,
            request.Email);

        if (result.Status == PersonnelOperationStatus.DuplicateEmail)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "A personnel profile with this email already exists.",
                Code: ErrorCodes.EmailAlreadyInUse,
                Details: $"The email '{request.Email}' is already assigned to another personnel profile."
            ));
        }

        var personnel = result.Personnel!;

        return CreatedAtAction(
            nameof(GetById),
            new { id = personnel.Id },
            personnel.ToResponse());
    }

    /// <summary>
    /// Retrieves all personnel profiles.
    /// </summary>
    /// <returns>A list of personnel profiles.</returns>
    /// <response code="200">The personnel profiles were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read personnel profiles.</response>
    [HttpGet]
    [HasPermission(Permissions.Personnels.Read)]
    [SwaggerOperation(
        Summary = "Get all personnel profiles",
        Description = "Returns all personnel profiles."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PersonnelListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<PersonnelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PersonnelResponse>>> GetAll()
    {
        var personnels = await _personnelService.GetAllAsync();
        return Ok(personnels.Select(p => p.ToResponse()));
    }

    /// <summary>
    /// Retrieves a personnel profile by identifier.
    /// </summary>
    /// <param name="id">The personnel identifier.</param>
    /// <returns>The matching personnel profile.</returns>
    /// <response code="200">The personnel profile was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read personnel profiles.</response>
    /// <response code="404">No personnel profile exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.Personnels.Read)]
    [SwaggerOperation(
        Summary = "Get a personnel profile by id",
        Description = "Returns a single personnel profile by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PersonnelResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(PersonnelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonnelResponse>> GetById(int id)
    {
        var personnel = await _personnelService.GetByIdAsync(id);
        if (personnel is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Personnel profile not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No personnel profile exists with id {id}."
            ));
        }
        return Ok(personnel.ToResponse());
    }

    /// <summary>
    /// Updates an existing personnel profile.
    /// </summary>
    /// <param name="id">The personnel identifier.</param>
    /// <param name="request">The updated personnel profile.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The personnel profile was updated successfully.</response>
    /// <response code="400">A personnel profile with the supplied email already exists.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update personnel profiles.</response>
    /// <response code="404">No personnel profile exists with the supplied identifier.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.Personnels.Update)]
    [SwaggerOperation(
        Summary = "Update a personnel profile",
        Description = "Updates an existing personnel profile by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdatePersonnelRequest), typeof(UpdatePersonnelRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DuplicatePersonnelEmailExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdatePersonnelRequest request)
    {
        var status = await _personnelService.UpdateAsync(
            id,
            request.FirstName,
            request.LastName,
            request.Function,
            request.Email);

        if (status == PersonnelOperationStatus.NotFound)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Personnel profile not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No personnel profile exists with id {id}."
            ));
        }

        if (status == PersonnelOperationStatus.DuplicateEmail)
        {
            return BadRequest(new ApiErrorResponse(
                Error: "A personnel profile with this email already exists.",
                Code: ErrorCodes.EmailAlreadyInUse,
                Details: $"The email '{request.Email}' is already assigned to another personnel profile."
            ));
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a personnel profile.
    /// </summary>
    /// <param name="id">The personnel identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The personnel profile was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete personnel profiles.</response>
    /// <response code="404">No personnel profile exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Personnels.Delete)]
    [SwaggerOperation(
        Summary = "Delete a personnel profile",
        Description = "Deletes an existing personnel profile by its identifier."
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
        var deleted = await _personnelService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Personnel profile not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No personnel profile exists with id {id}."
            ));
        }

        return NoContent();
    }
}
