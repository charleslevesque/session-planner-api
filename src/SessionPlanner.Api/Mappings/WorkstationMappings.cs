using SessionPlanner.Api.Dtos.Workstations;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class WorkstationMappings
{
    public static WorkstationResponse ToResponse(this Workstation workstation)
        => new(
            workstation.Id,
            workstation.LaboratoryId,
            workstation.OperatingSystemId,
            workstation.OperatingSystem.Name,
            workstation.Count
        );
}
