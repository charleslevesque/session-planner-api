using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.Auth;

public record LoginRequest(
    [Required, EmailAddress]
    string Email,

    [Required, MinLength(8)]
    string Password
);
