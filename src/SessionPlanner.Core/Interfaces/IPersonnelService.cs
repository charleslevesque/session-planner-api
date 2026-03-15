using PersonnelEntity = SessionPlanner.Core.Entities.Personnel;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Core.Interfaces;

public enum PersonnelOperationStatus
{
    Success,
    NotFound,
    DuplicateEmail
}

public record PersonnelOperationResult(PersonnelOperationStatus Status, PersonnelEntity? Personnel);

public interface IPersonnelService
{
    Task<PersonnelOperationResult> CreateAsync(string firstName, string lastName, PersonnelFunction function, string email);
    Task<List<PersonnelEntity>> GetAllAsync();
    Task<PersonnelEntity?> GetByIdAsync(int id);
    Task<PersonnelOperationStatus> UpdateAsync(int id, string firstName, string lastName, PersonnelFunction function, string email);
    Task<bool> DeleteAsync(int id);
}