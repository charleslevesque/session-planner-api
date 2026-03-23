using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.EquipmentModels;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.EquipmentModels;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Equipment Models")]
public class EquipmentModelsController : ControllerBase
{
    private readonly IEquipmentModelService _equipmentModelService;

    public EquipmentModelsController(IEquipmentModelService equipmentModelService)
    {
        _equipmentModelService = equipmentModelService;
    }

    /// <summary>
    /// Creates an equipment model.
    /// </summary>
    /// <remarks>
    /// Creates a new equipment model using the supplied name, quantity, default accessories, and notes.
    /// </remarks>
    /// <param name="request">The equipment model details.</param>
    /// <returns>The newly created equipment model.</returns>
    /// <response code="201">The equipment model was created successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create equipment models.</response>
    [HttpPost]
    [HasPermission(Permissions.EquipmentModels.Create)]
    [SwaggerOperation(
        Summary = "Create an equipment model",
        Description = "Creates a new equipment model and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateEquipmentModelRequest), typeof(CreateEquipmentModelRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(EquipmentModelResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(EquipmentModelResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EquipmentModelResponse>> Create(CreateEquipmentModelRequest request)
    {
        var equipment = await _equipmentModelService.CreateAsync(
            request.Name,
            request.Quantity,
            request.DefaultAccessories,
            request.Notes);

        return CreatedAtAction(
            nameof(GetById),
            new { id = equipment.Id },
            equipment.ToResponse());
    }

    /// <summary>
    /// Retrieves all equipment models.
    /// </summary>
    /// <returns>A list of equipment models.</returns>
    /// <response code="200">The equipment models were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read equipment models.</response>
    [HttpGet]
    [HasPermission(Permissions.EquipmentModels.Read)]
    [SwaggerOperation(
        Summary = "Get all equipment models",
        Description = "Returns all equipment models."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EquipmentModelListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<EquipmentModelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]

    public async Task<ActionResult<IEnumerable<EquipmentModelResponse>>> GetAll()
    {
        var equipments = await _equipmentModelService.GetAllAsync();
        return Ok(equipments.Select(e => e.ToResponse()));
    }

    /// <summary>
    /// Retrieves an equipment model by identifier.
    /// </summary>
    /// <param name="id">The equipment model identifier.</param>
    /// <returns>The matching equipment model.</returns>
    /// <response code="200">The equipment model was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read equipment models.</response>
    /// <response code="404">No equipment model exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.EquipmentModels.Read)]
    [SwaggerOperation(
        Summary = "Get an equipment model by id",
        Description = "Returns a single equipment model by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(EquipmentModelResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(EquipmentModelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]

    public async Task<ActionResult<EquipmentModelResponse>> GetById(int id)
    {
        var equipment = await _equipmentModelService.GetByIdAsync(id);
        
        if (equipment is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Equipment model not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No equipment model exists with id {id}."
            ));
        }

        return Ok(equipment.ToResponse());
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.EquipmentModels.Update)]

    public async Task<IActionResult> Update(int id, UpdateEquipmentModelRequest request)
    {
        var updated = await _equipmentModelService.UpdateAsync(
            id,
            request.Name,
            request.Quantity,
            request.DefaultAccessories,
            request.Notes);

        if (!updated)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Equipment model not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No equipment model exists with id {id}."
            ));
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes an equipment model.
    /// </summary>
    /// <param name="id">The equipment model identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The equipment model was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete equipment models.</response>
    /// <response code="404">No equipment model exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.EquipmentModels.Delete)]
    [SwaggerOperation(
        Summary = "Delete an equipment model",
        Description = "Deletes an existing equipment model by its identifier."
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
        var deleted = await _equipmentModelService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "Equipment model not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No equipment model exists with id {id}."
            ));
        }

        return NoContent();
    }
}
