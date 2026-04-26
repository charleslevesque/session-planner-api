using SessionPlanner.Core.Enums;

namespace SessionPlanner.Api.Dtos.Personnel;

public record UpdatePersonnelRequest(string FirstName, string LastName, PersonnelFunction Function, string Email);
