using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Dtos.Personnel;

public record CreatePersonnelRequest(string FirstName, string LastName, PersonnelFunction Function, string Email);
