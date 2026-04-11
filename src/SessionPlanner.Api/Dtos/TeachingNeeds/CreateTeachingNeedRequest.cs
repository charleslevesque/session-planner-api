using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record CreateTeachingNeedRequest(
    [Required] int CourseId,
    // Required when the caller is Admin or Technician. Ignored for Teacher role (uses own personnel).
    int? PersonnelId,
    string? Notes,
    int? ExpectedStudents,
    bool? HasTechNeeds,
    bool? FoundAllCourses,
    string? DesiredModifications,
    bool? AllowsUpdates,
    string? AdditionalComments);
