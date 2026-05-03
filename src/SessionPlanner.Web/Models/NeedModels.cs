namespace SessionPlanner.Web.Models;

public record TeachingNeedResponse(
    int Id, int SessionId, int PersonnelId, string PersonnelFullName,
    int CourseId, string CourseCode, string? CourseName,
    string Status, string CreatedAt, string? SubmittedAt, string? ReviewedAt,
    int? ReviewedByUserId, string? RejectionReason, string? Notes,
    int? ExpectedStudents, bool? HasTechNeeds, bool? FoundAllCourses,
    string? DesiredModifications, bool? AllowsUpdates, string? AdditionalComments,
    List<TeachingNeedItemResponse> Items, bool IsFastTrack);

public record TeachingNeedItemResponse(
    int Id, string ItemType, int? SoftwareId, string? SoftwareName,
    int? SoftwareVersionId, string? SoftwareVersionNumber, int? OsId, string? OsName,
    int? Quantity, string? Description, string? Notes, string? DetailsJson,
    bool? AlreadyInstalledInLabs);

public record MyNeedResponse(
    int Id, int SessionId, string SessionTitle,
    int CourseId, string CourseCode, string? CourseName,
    string Status, string CreatedAt, string? SubmittedAt, string? ReviewedAt,
    string? RejectionReason, string? Notes);

public record CreateTeachingNeedRequest(
    int CourseId, int? PersonnelId, string? Notes, int? ExpectedStudents,
    bool? HasTechNeeds, bool? FoundAllCourses, string? DesiredModifications,
    bool? AllowsUpdates, string? AdditionalComments);

public record UpdateTeachingNeedRequest(
    string? Notes, int? ExpectedStudents,
    bool? HasTechNeeds, bool? FoundAllCourses, string? DesiredModifications,
    bool? AllowsUpdates, string? AdditionalComments);

public record AddNeedItemRequest(
    string? ItemType, int? SoftwareId, int? SoftwareVersionId, int? OsId,
    int? Quantity, string? Description, string? Notes, string? DetailsJson);

public record RejectNeedRequest(string RejectionReason);
public record SubmitNeedResponse(TeachingNeedResponse Need, List<string> Warnings);
public record NeedHistoryEntry(int Id, int NeedId, string Status, string ChangedAt, string? ChangedByName, string? Notes);
