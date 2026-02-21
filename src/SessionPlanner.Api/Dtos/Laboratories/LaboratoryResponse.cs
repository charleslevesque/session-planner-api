using SessionPlanner.Api.Dtos.Workstations;

namespace SessionPlanner.Api.Dtos.Laboratories;

public record LaboratoryResponse(
    int Id,
    string Name,
    string Building,
    int NumberOfPCs,
    int SeatingCapacity,
    ICollection<WorkstationResponse> Workstations
);