using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class EquipmentModelService : IEquipmentModelService
{
    private readonly AppDbContext _db;

    public EquipmentModelService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EquipmentModel> CreateAsync(string name, int quantity, string? defaultAccessories, string? notes)
    {
        var equipment = new EquipmentModel
        {
            Name = name,
            Quantity = quantity,
            DefaultAccessories = defaultAccessories,
            Notes = notes
        };

        _db.EquipmentModels.Add(equipment);
        await _db.SaveChangesAsync();

        return equipment;
    }

    public async Task<List<EquipmentModel>> GetAllAsync()
    {
        return await _db.EquipmentModels.ToListAsync();
    }

    public async Task<EquipmentModel?> GetByIdAsync(int id)
    {
        return await _db.EquipmentModels.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(int id, string name, int quantity, string? defaultAccessories, string? notes)
    {
        var equipment = await _db.EquipmentModels.FindAsync(id);
        if (equipment is null)
            return false;

        equipment.Name = name;
        equipment.Quantity = quantity;
        equipment.DefaultAccessories = defaultAccessories;
        equipment.Notes = notes;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var equipment = await _db.EquipmentModels.FindAsync(id);
        if (equipment is null)
            return false;

        _db.EquipmentModels.Remove(equipment);
        await _db.SaveChangesAsync();

        return true;
    }
}