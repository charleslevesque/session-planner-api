using Microsoft.AspNetCore.Mvc;
using SessionPlanner.Api.Dtos.SaaSProducts;
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
public class SaaSProductsController : ControllerBase
{
    private readonly ISaaSProductService _saaSProductService;

    public SaaSProductsController(ISaaSProductService saaSProductService)
    {
        _saaSProductService = saaSProductService;
    }

    [HttpPost]
    public async Task<ActionResult<SaaSProductResponse>> Create(CreateSaaSProductRequest request)
    {
        var product = await _saaSProductService.CreateAsync(request.Name, request.NumberOfAccounts, request.Notes);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaaSProductResponse>>> GetAll()
    {
        var products = await _saaSProductService.GetAllAsync();
        return Ok(products.Select(p => p.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaaSProductResponse>> GetById(int id)
    {
        var product = await _saaSProductService.GetByIdAsync(id);
        if (product is null)
            return NotFound();
        return Ok(product.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSaaSProductRequest request)
    {
        var updated = await _saaSProductService.UpdateAsync(id, request.Name, request.NumberOfAccounts, request.Notes);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _saaSProductService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
