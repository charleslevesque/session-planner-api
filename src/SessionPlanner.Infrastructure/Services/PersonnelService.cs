using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Enums;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;
using PersonnelEntity = SessionPlanner.Core.Entities.Personnel;

namespace SessionPlanner.Infrastructure.Services;

public class PersonnelService : IPersonnelService
{
    private readonly AppDbContext _db;

    public PersonnelService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PersonnelOperationResult> CreateAsync(string firstName, string lastName, PersonnelFunction function, string email)
    {
        var existingPersonnel = await _db.Personnel
            .FirstOrDefaultAsync(p => p.Email == email);

        if (existingPersonnel is not null)
            return new PersonnelOperationResult(PersonnelOperationStatus.DuplicateEmail, null);

        var personnel = new PersonnelEntity
        {
            FirstName = firstName,
            LastName = lastName,
            Function = function,
            Email = email
        };

        _db.Personnel.Add(personnel);
        await _db.SaveChangesAsync();

        return new PersonnelOperationResult(PersonnelOperationStatus.Success, personnel);
    }

    public async Task<List<PersonnelEntity>> GetAllAsync()
    {
        return await _db.Personnel.ToListAsync();
    }

    public async Task<PersonnelEntity?> GetByIdAsync(int id)
    {
        return await _db.Personnel.FindAsync(id);
    }

    public async Task<PersonnelOperationStatus> UpdateAsync(int id, string firstName, string lastName, PersonnelFunction function, string email)
    {
        var personnel = await _db.Personnel.FindAsync(id);

        if (personnel is null)
            return PersonnelOperationStatus.NotFound;

        var existingPersonnel = await _db.Personnel
            .FirstOrDefaultAsync(p => p.Email == email && p.Id != id);

        if (existingPersonnel is not null)
            return PersonnelOperationStatus.DuplicateEmail;

        personnel.FirstName = firstName;
        personnel.LastName = lastName;
        personnel.Function = function;
        personnel.Email = email;

        await _db.SaveChangesAsync();

        return PersonnelOperationStatus.Success;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var personnel = await _db.Personnel.FindAsync(id);

        if (personnel is null)
            return false;

        _db.Personnel.Remove(personnel);
        await _db.SaveChangesAsync();

        return true;
    }
}