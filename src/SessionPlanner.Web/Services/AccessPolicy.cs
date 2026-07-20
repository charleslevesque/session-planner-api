using SessionPlanner.Web.Models;

namespace SessionPlanner.Web.Services;

public static class AccessPolicy
{
    public static readonly string[] AllAuthenticated =
    [
        Roles.Admin,
        Roles.Professor,
        Roles.LabInstructor,
        Roles.CourseInstructor
    ];

    public static readonly string[] Technical =
    [
        Roles.Admin,
        Roles.LabInstructor
    ];

    public static readonly string[] Teachers =
    [
        Roles.Professor,
        Roles.CourseInstructor
    ];

    public static readonly string[] NeedAuthors =
    [
        Roles.Professor,
        Roles.CourseInstructor,
        Roles.LabInstructor
    ];

    public static bool HasRole(string? role, IReadOnlyCollection<string> allowedRoles) =>
        !string.IsNullOrWhiteSpace(role) && allowedRoles.Contains(role);
}
