using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Dtos.Workstations;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class LaboratoryMappings
{
    public static LaboratoryResponse ToResponse(this Laboratory lab)
        => new(
            lab.Id,
            lab.Name,
            lab.Building,
            lab.NumberOfPCs,
            lab.SeatingCapacity,
            lab.Workstations.Select(w => w.ToResponse()).ToList()
        );

    public static Laboratory ToEntity(this CreateLaboratoryRequest r)
        => new()
        {
            Name = r.Name,
            Building = r.Building,
            NumberOfPCs = r.NumberOfPCs,
            SeatingCapacity = r.SeatingCapacity
        };

    public static void Apply(this UpdateLaboratoryRequest r, Laboratory lab)
    {
        lab.Name = r.Name;
        lab.Building = r.Building;
        lab.NumberOfPCs = r.NumberOfPCs;
        lab.SeatingCapacity = r.SeatingCapacity;
    }
}