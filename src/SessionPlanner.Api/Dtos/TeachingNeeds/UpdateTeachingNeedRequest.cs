using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record UpdateTeachingNeedRequest(
    [Required] int CourseId,
    string? Notes,
    int? ExpectedStudents,
    bool? HasTechNeeds,
    bool? FoundAllCourses,
    string? DesiredModifications,
    bool? AllowsUpdates,
    string? AdditionalComments);
