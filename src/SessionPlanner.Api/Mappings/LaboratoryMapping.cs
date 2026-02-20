using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class LaboratoryMappings
{
    public static LaboratoryResponse ToResponse(this Laboratory lab)
        => new(lab.Id, lab.Name);

     public static Laboratory toEntity(this CreateLaboratoryRequest r)
        => new()
        {
           Name = r.Name
        };

    public static void Apply(this UpdateLaboratoryRequest r, Laboratory lab)
    {
        lab.Name = r.Name;
    }
}