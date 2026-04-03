namespace SessionPlanner.Api.Dtos.Configurations;

public record UpdateConfigurationRequest(string Title, IReadOnlyList<int> OSIds, IReadOnlyList<int> LaboratoryIds, string? Notes);
