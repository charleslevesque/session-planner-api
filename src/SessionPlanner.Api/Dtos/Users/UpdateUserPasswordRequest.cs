using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.Users;

public record UpdateUserPasswordRequest(
    [Required, MinLength(8)]
    string NewPassword
);
