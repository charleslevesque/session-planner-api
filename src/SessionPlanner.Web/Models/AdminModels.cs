namespace SessionPlanner.Web.Models;

public record CourseResponse(int Id, string Code, string? Name, string? Description);
public record CreateCourseRequest(string Code, string? Name, string? Description);
public record UpdateCourseRequest(string Code, string? Name, string? Description);

public record SaasProductResponse(int Id, string Name, string? Description, string? Url);
public record CreateSaasProductRequest(string Name, string? Description, string? Url);

public record ConfigurationResponse(int Id, string Name, string? Description);
public record CreateConfigurationRequest(string Name, string? Description);

public record VirtualMachineResponse(int Id, string Name, string? Description, int? CpuCores, int? RamGb, int? StorageGb, string? OperatingSystem);
public record CreateVirtualMachineRequest(string Name, string? Description, int? CpuCores, int? RamGb, int? StorageGb, string? OperatingSystem);

public record PhysicalServerResponse(int Id, string Name, string? Description, string? Specifications);
public record CreatePhysicalServerRequest(string Name, string? Description, string? Specifications);

public record EquipmentModelResponse(int Id, string Name, string? Description, string? Manufacturer, int? AvailableQuantity);
public record CreateEquipmentModelRequest(string Name, string? Description, string? Manufacturer, int? AvailableQuantity);

public record SoftwareCatalogEntry(int Id, string Name, string? Publisher, List<string> Versions);
