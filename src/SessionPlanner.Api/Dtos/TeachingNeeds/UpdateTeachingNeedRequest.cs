using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record UpdateTeachingNeedRequest(
    [Required] int CourseId,
    string? Notes);
