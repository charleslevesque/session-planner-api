using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.Configurations;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.OpenApi.Examples.Configurations;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Common;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Configurations")]
public class ConfigurationsController : ControllerBase
{
    private readonly IConfigurationService _configurationService;

    public ConfigurationsController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    /// <summary>
    /// Creates a configuration.
    /// </summary>
    /// <remarks>
    /// Creates a new configuration record using the supplied title and optional notes.
    /// </remarks>
    /// <param name="request">The configuration details.</param>
    /// <returns>The newly created configuration.</returns>
    /// <response code="201">The configuration was created successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpPost]
    [HasPermission(Permissions.Configurations.Create)]
    [SwaggerOperation(
        Summary = "Create a configuration",
        Description = "Creates a new configuration and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateConfigurationRequest), typeof(CreateConfigurationRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(ConfigurationResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(ConfigurationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ConfigurationResponse>> Create(CreateConfigurationRequest request)
    {
        var configuration = await _configurationService.CreateAsync(request.Title, request.Notes);

        return CreatedAtAction(
            nameof(GetById),
            new { id = configuration.Id },
            configuration.ToResponse());
    }

    /// <summary>
    /// Retrieves all configurations.
    /// </summary>
    /// <returns>A list of configurations.</returns>
    /// <response code="200">The configurations were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpGet]
    [HasPermission(Permissions.Configurations.Read)]
    [SwaggerOperation(
        Summary = "Get all configurations",
        Description = "Returns all configuration records."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ConfigurationListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ConfigurationResponse>>> GetAll()
    {
        var configurations = await _configurationService.GetAllAsync();
        return Ok(configurations.Select(c => c.ToResponse()));
    }

    /// <summary>
    /// Retrieves a configuration by identifier.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <returns>The matching configuration.</returns>
    /// <response code="200">The configuration was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">No configuration exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.Configurations.Read)]
    [SwaggerOperation(
        Summary = "Get a configuration by id",
        Description = "Returns a single configuration by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ConfigurationResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(ConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ConfigurationResponse>> GetById(int id)
    {
        var configuration = await _configurationService.GetByIdAsync(id);
        if (configuration is null)
            return NotFound(new ApiErrorResponse(
                Error: "The requested resource was not found.",
                Code: ErrorCodes.NotFound
            ));
        return Ok(configuration.ToResponse());
    }

    /// <summary>
    /// Updates an existing configuration.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <param name="request">The updated configuration data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The configuration was updated successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">No configuration exists with the supplied identifier.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.Configurations.Update)]
    [SwaggerOperation(
        Summary = "Update a configuration",
        Description = "Updates an existing configuration by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdateConfigurationRequest), typeof(UpdateConfigurationRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(int id, UpdateConfigurationRequest request)
    {
        var updated = await _configurationService.UpdateAsync(id, request.Title, request.Notes);

        if (!updated)
            return NotFound(new ApiErrorResponse(
                Error: "The requested resource was not found.",
                Code: ErrorCodes.NotFound
            ));

        return NoContent();
    }

    /// <summary>
    /// Deletes a configuration.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The configuration was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">No configuration exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Configurations.Delete)]
    [SwaggerOperation(
        Summary = "Delete a configuration",
        Description = "Deletes an existing configuration by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _configurationService.DeleteAsync(id);

        if (!deleted)
            return NotFound(new ApiErrorResponse(
                Error: "The requested resource was not found.",
                Code: ErrorCodes.NotFound
            ));


        return NoContent();
    }
}
