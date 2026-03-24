using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.SaaSProducts;
using SessionPlanner.Api.Dtos.Common;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using SessionPlanner.Core.Auth;
using SessionPlanner.Api.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Api.Common;
using SessionPlanner.Api.OpenApi.Examples.SaaSProducts;
using SessionPlanner.Api.OpenApi.Examples.Common;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
[Tags("SaaS Products")]
public class SaaSProductsController : ControllerBase
{
    private readonly ISaaSProductService _saaSProductService;

    public SaaSProductsController(ISaaSProductService saaSProductService)
    {
        _saaSProductService = saaSProductService;
    }

    /// <summary>
    /// Creates a SaaS product.
    /// </summary>
    /// <param name="request">The SaaS product details, including it's name, the number of accounts, and the optional notes.</param>
    /// <returns>The newly created SaaS product.</returns>
    /// <response code="201">The SaaS product was created successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to create SaaS products.</response>
    [HttpPost]
    [HasPermission(Permissions.SaaSProducts.Create)]
    [SwaggerOperation(
        Summary = "Create a SaaS product",
        Description = "Creates a new SaaS product and returns the created resource."
    )]
    [SwaggerRequestExample(typeof(CreateSaaSProductRequest), typeof(CreateSaaSProductRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(SaaSProductResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(SaaSProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SaaSProductResponse>> Create(CreateSaaSProductRequest request)
    {
        var product = await _saaSProductService.CreateAsync(request.Name, request.NumberOfAccounts, request.Notes);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product.ToResponse());
    }

    /// <summary>
    /// Retrieves all SaaS products.
    /// </summary>
    /// <returns>A list of SaaS products.</returns>
    /// <response code="200">The SaaS products were retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read SaaS products.</response>
    [HttpGet]
    [HasPermission(Permissions.SaaSProducts.Read)]
    [SwaggerOperation(
        Summary = "Get all SaaS products",
        Description = "Returns all SaaS products."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SaaSProductListResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [ProducesResponseType(typeof(IEnumerable<SaaSProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SaaSProductResponse>>> GetAll()
    {
        var products = await _saaSProductService.GetAllAsync();
        return Ok(products.Select(p => p.ToResponse()));
    }

     /// <summary>
    /// Retrieves a SaaS product by identifier.
    /// </summary>
    /// <param name="id">The SaaS product identifier.</param>
    /// <returns>The matching SaaS product.</returns>
    /// <response code="200">The SaaS product was found.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to read SaaS products.</response>
    /// <response code="404">No SaaS product exists with the supplied identifier.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.SaaSProducts.Read)]
    [SwaggerOperation(
        Summary = "Get a SaaS product by id",
        Description = "Returns a single SaaS product by its identifier."
    )]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SaaSProductResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(typeof(SaaSProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaaSProductResponse>> GetById(int id)
    {
        var product = await _saaSProductService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound(new ApiErrorResponse(
                Error: "SaaS product not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No SaaS product exists with id {id}."
            ));
        }
        return Ok(product.ToResponse());
    }

    /// <summary>
    /// Updates an existing SaaS product.
    /// </summary>
    /// <param name="id">The SaaS product identifier.</param>
    /// <param name="request">The updated SaaS product data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The SaaS product was updated successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to update SaaS products.</response>
    /// <response code="404">No SaaS product exists with the supplied identifier.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.SaaSProducts.Update)]
    [SwaggerOperation(
        Summary = "Update a SaaS product",
        Description = "Updates an existing SaaS product by its identifier."
    )]
    [SwaggerRequestExample(typeof(UpdateSaaSProductRequest), typeof(UpdateSaaSProductRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(ForbiddenErrorExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundErrorExample))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateSaaSProductRequest request)
    {
        var updated = await _saaSProductService.UpdateAsync(id, request.Name, request.NumberOfAccounts, request.Notes);

        if (!updated)
        {
            return NotFound(new ApiErrorResponse(
                Error: "SaaS product not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No SaaS product exists with id {id}."
            ));
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a SaaS product.
    /// </summary>
    /// <param name="id">The SaaS product identifier.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The SaaS product was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not allowed to delete SaaS products.</response>
    /// <response code="404">No SaaS product exists with the supplied identifier.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.SaaSProducts.Delete)]
    [SwaggerOperation(
        Summary = "Delete a SaaS product",
        Description = "Deletes an existing SaaS product by its identifier."
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
        var deleted = await _saaSProductService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiErrorResponse(
                Error: "SaaS product not found.",
                Code: ErrorCodes.NotFound,
                Details: $"No SaaS product exists with id {id}."
            ));
        }

        return NoContent();
    }
}
