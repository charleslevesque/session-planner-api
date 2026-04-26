using SessionPlanner.Core.Enums;

namespace SessionPlanner.Api.Dtos.Personnel;

public record PersonnelResponse(int Id, string FirstName, string LastName, PersonnelFunction Function, string Email);
