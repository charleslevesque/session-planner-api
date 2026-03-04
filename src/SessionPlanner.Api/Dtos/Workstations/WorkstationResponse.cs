namespace SessionPlanner.Api.Dtos.Workstations;

public record WorkstationResponse(
    int Id,
    string Name,
    int LaboratoryId,
    int OSId,
    string OSName
);
