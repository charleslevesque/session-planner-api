using SessionPlanner.Core.Enums;

namespace SessionPlanner.Api.Dtos.Personnel;

public record CreatePersonnelRequest(string FirstName, string LastName, PersonnelFunction Function, string Email);
