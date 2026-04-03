namespace SessionPlanner.Api.Dtos.Configurations;

public record ConfigurationResponse(int Id, string Title, IReadOnlyList<int> OSIds, IReadOnlyList<int> LaboratoryIds, string? Notes);
