namespace SessionPlanner.Api.Dtos.EquipmentModels;

public record UpdateEquipmentModelRequest(string Name, int Quantity, string? DefaultAccessories, string? Notes);
