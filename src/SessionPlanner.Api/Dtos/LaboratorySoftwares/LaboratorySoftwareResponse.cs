namespace SessionPlanner.Api.Dtos.LaboratorySoftwares;

public record LaboratorySoftwareResponse(
    int LaboratoryId,
    string LaboratoryName,
    int SoftwareId,
    string SoftwareName,
    string Status
);
