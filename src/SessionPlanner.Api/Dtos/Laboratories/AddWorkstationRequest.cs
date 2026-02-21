namespace SessionPlanner.Api.Dtos.Laboratories;

public record AddWorkstationRequest(
    int OperatingSystemId,
    int Count
);
