namespace SessionPlanner.Api.Dtos.EquipmentModels;

public record CreateEquipmentModelRequest(string Name, int Quantity, string? DefaultAccessories, string? Notes);
