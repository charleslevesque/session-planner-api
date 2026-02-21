namespace SessionPlanner.Api.Dtos.Laboratories;

public record CreateLaboratoryRequest(
    string Name,
    string Building,
    int NumberOfPCs,
    int SeatingCapacity
);