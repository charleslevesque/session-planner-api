namespace SessionPlanner.Api.Dtos.SaaSProducts;

public record CreateSaaSProductRequest(string Name, int? NumberOfAccounts, string? Notes);
