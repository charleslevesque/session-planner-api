using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.Users;

public record UpdateCurrentUserEmailRequest(
    [Required, EmailAddress, MaxLength(100)]
    string NewEmail,
    [Required, MinLength(8)]
    string CurrentPassword
);