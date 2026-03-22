namespace SessionPlanner.Api.Common;

public static class ErrorCodes
{
    
    public const string BadRequest = "BAD_REQUEST";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";

    // AuthController Errors
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
    public const string AccountAlreadyExists = "ACCOUNT_ALREADY_EXISTS";
    public const string CurrentUserNotFound = "CURRENT_USER_NOT_FOUND";

    // UserController Errors
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string UsernameAlreadyExists = "USERNAME_ALREADY_EXISTS";
    public const string RoleNotFound = "ROLE_NOT_FOUND";
    public const string EmailAlreadyInUse = "EMAIL_ALREADY_IN_USE";

    // SessionsController Errors
    public const string SessionNotFound = "SESSION_NOT_FOUND";
    public const string InvalidSessionDates = "INVALID_SESSION_DATES";
    public const string InvalidSessionTransition = "INVALID_SESSION_TRANSITION";
    public const string SessionDeleteNotAllowed = "SESSION_DELETE_NOT_ALLOWED";

    // TeachingNeedsController Errors
    public const string TeachingNeedNotFound = "TEACHING_NEED_NOT_FOUND";
    public const string TeachingNeedConflict = "TEACHING_NEED_CONFLICT";
    public const string TeachingNeedForbidden = "TEACHING_NEED_FORBIDDEN";
    public const string ItemNotFound = "ITEM_NOT_FOUND";
    public const string RejectionReasonRequired = "REJECTION_REASON_REQUIRED";
}