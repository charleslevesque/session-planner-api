using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.Auth;

public record RegisterRequest(
    [Required, EmailAddress]
    string Email,

    [Required, MinLength(8)]
    string Password,

    [Required, MaxLength(100)]
    string FirstName,

    [Required, MaxLength(100)]
    string LastName,

    [MaxLength(30)]
    string? Role = null
);
