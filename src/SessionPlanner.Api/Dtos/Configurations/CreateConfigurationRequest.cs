namespace SessionPlanner.Api.Dtos.Configurations;

public record CreateConfigurationRequest(string Title, IReadOnlyList<int> OSIds, IReadOnlyList<int> LaboratoryIds, string? Notes);
