namespace SessionPlanner.Api.Dtos.Common;

public sealed record ApiErrorResponse(string Error, string? Code = null, string? Details = null);