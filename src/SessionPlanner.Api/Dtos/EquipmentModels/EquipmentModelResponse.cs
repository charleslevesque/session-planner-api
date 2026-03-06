namespace SessionPlanner.Api.Dtos.EquipmentModels;

public record EquipmentModelResponse(int Id, string Name, int Quantity, string? DefaultAccessories, string? Notes);
