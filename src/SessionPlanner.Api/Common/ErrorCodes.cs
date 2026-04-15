namespace SessionPlanner.Api.Common;

public static class ErrorCodes
{
    
    //Generic Errors
    public const string BadRequest = "BAD_REQUEST";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";

    // AuthController Errors
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string AccountDeactivated = "ACCOUNT_DEACTIVATED";
    public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";

    // UserController Errors
    public const string RoleNotFound = "ROLE_NOT_FOUND";
    public const string EmailAlreadyInUse = "EMAIL_ALREADY_IN_USE";

    // SessionsController Errors
    public const string InvalidSessionDates = "INVALID_SESSION_DATES";
    public const string InvalidSessionTransition = "INVALID_SESSION_TRANSITION";
    public const string SessionDeleteNotAllowed = "SESSION_DELETE_NOT_ALLOWED";

    public const string SessionCoursesNotModifiable = "SESSION_COURSES_NOT_MODIFIABLE";

    // TeachingNeedsController Errors
    public const string RejectionReasonRequired = "REJECTION_REASON_REQUIRED";
    public const string InvalidTeachingNeedTransition = "INVALID_TEACHING_NEED_TRANSITION";
}