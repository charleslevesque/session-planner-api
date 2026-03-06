using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.SaaSProducts;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SaaSProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SaaSProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<SaaSProductResponse>> Create(CreateSaaSProductRequest request)
    {
        var product = new SaaSProduct
        {
            Name = request.Name,
            NumberOfAccounts = request.NumberOfAccounts,
            Notes = request.Notes
        };

        _db.SaaSProducts.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaaSProductResponse>>> GetAll()
    {
        var products = await _db.SaaSProducts.ToListAsync();
        return Ok(products.Select(p => p.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaaSProductResponse>> GetById(int id)
    {
        var product = await _db.SaaSProducts.FindAsync(id);
        if (product is null)
            return NotFound();
        return Ok(product.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSaaSProductRequest request)
    {
        var product = await _db.SaaSProducts.FindAsync(id);

        if (product is null)
            return NotFound();

        product.Name = request.Name;
        product.NumberOfAccounts = request.NumberOfAccounts;
        product.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.SaaSProducts.FindAsync(id);

        if (product is null)
            return NotFound();

        _db.SaaSProducts.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
