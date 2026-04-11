using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.Sessions;

public record UpdateSessionCoursesRequest(
    [Required]
    IReadOnlyList<int> CourseIds
);
