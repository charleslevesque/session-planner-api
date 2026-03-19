using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.Auth;

public record ChangePasswordRequest(
    [Required, MinLength(8)]
    string CurrentPassword,

    [Required, MinLength(8)]
    string NewPassword
);
