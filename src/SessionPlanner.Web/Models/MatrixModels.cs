namespace SessionPlanner.Web.Models;

public record LaboratoryResponse(int Id, string Name, string? Description);
public record SoftwareResponse(int Id, string Name, string? Description, string? Publisher, List<SoftwareVersionResponse>? Versions);
public record SoftwareVersionResponse(int Id, int SoftwareId, string VersionNumber, string? Notes);
public record LaboratorySoftwareResponse(int Id, int LaboratoryId, string LaboratoryName, int SoftwareId, string SoftwareName, int? SoftwareVersionId, string? SoftwareVersionNumber, bool IsInstalled);
public record OperatingSystemResponse(int Id, string Name, string? Version);
