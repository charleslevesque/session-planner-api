namespace SessionPlanner.Api.Dtos.Workstations;

public record WorkstationResponse(
    int Id,
    int LaboratoryId,
    int OperatingSystemId,
    string OperatingSystemName,
    int Count
);
