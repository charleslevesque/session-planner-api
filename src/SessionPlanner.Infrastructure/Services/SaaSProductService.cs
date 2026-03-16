using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class SaaSProductService : ISaaSProductService
{
    private readonly AppDbContext _db;

    public SaaSProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SaaSProduct> CreateAsync(string name, int? numberOfAccounts, string? notes)
    {
        var product = new SaaSProduct
        {
            Name = name,
            NumberOfAccounts = numberOfAccounts,
            Notes = notes
        };

        _db.SaaSProducts.Add(product);
        await _db.SaveChangesAsync();

        return product;
    }

    public async Task<List<SaaSProduct>> GetAllAsync()
    {
        return await _db.SaaSProducts.ToListAsync();
    }

    public async Task<SaaSProduct?> GetByIdAsync(int id)
    {
        return await _db.SaaSProducts.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(int id, string name, int? numberOfAccounts, string? notes)
    {
        var product = await _db.SaaSProducts.FindAsync(id);
        if (product is null)
            return false;

        product.Name = name;
        product.NumberOfAccounts = numberOfAccounts;
        product.Notes = notes;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.SaaSProducts.FindAsync(id);
        if (product is null)
            return false;

        _db.SaaSProducts.Remove(product);
        await _db.SaveChangesAsync();

        return true;
    }
}