namespace SessionPlanner.Api.Dtos.SaaSProducts;

public record UpdateSaaSProductRequest(string Name, int? NumberOfAccounts, string? Notes);
