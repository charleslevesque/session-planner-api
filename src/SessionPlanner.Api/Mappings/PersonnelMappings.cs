using SessionPlanner.Api.Dtos.Personnel;
using PersonnelEntity = SessionPlanner.Core.Entities.Personnel;

namespace SessionPlanner.Api.Mappings;

public static class PersonnelMappings
{
    public static PersonnelResponse ToResponse(this PersonnelEntity personnel)
    {
        return new PersonnelResponse(
            personnel.Id,
            personnel.FirstName,
            personnel.LastName,
            personnel.Function,
            personnel.Email
        );
    }
}
