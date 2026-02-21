namespace SessionPlanner.Api.Dtos.Laboratories;

public record UpdateLaboratoryRequest(
    string Name,
    string Building,
    int NumberOfPCs,
    int SeatingCapacity
);