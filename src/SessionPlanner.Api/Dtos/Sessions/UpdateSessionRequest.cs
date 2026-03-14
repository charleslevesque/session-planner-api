using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.Sessions;

public record UpdateSessionRequest(
    [Required, MaxLength(200)]
    string Title,

    [Required]
    DateTime StartDate,

    [Required]
    DateTime EndDate
);
