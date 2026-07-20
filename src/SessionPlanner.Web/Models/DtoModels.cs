using System.Text.Json.Serialization;

namespace SessionPlanner.Web.Models;

public static class Roles
{
    public const string Admin = "admin";
    public const string Professor = "professor";
    public const string LabInstructor = "lab_instructor";
    public const string CourseInstructor = "course_instructor";

    public static readonly string[] All =
    [
        Admin,
        Professor,
        LabInstructor,
        CourseInstructor
    ];

    public static string Label(string? role) => role switch
    {
        Admin => "Administrateur",
        Professor => "Professeur(e)",
        LabInstructor => "Charge(e) de laboratoire",
        CourseInstructor => "Charge(e) de cours",
        _ => "Utilisateur"
    };
}

public static class NeedItemTypes
{
    public const string SaaS = "saas";
    public const string Software = "software";
    public const string Configuration = "configuration";
    public const string VirtualMachine = "virtual_machine";
    public const string PhysicalServer = "physical_server";
    public const string EquipmentLoan = "equipment_loan";
    public const string Other = "other";

    public static readonly string[] TeacherTypes =
    [
        SaaS,
        Software,
        Configuration,
        VirtualMachine,
        PhysicalServer,
        EquipmentLoan
    ];

    public static string Label(string? type) => type switch
    {
        SaaS => "SaaS",
        Software => "Logiciels",
        Configuration => "Configurations",
        VirtualMachine => "Machines virtuelles",
        PhysicalServer => "Serveurs physiques",
        EquipmentLoan => "Prets d'equipement",
        Other => "Autre besoin",
        _ => type ?? "Besoin"
    };
}

public static class StatusLabels
{
    public static string Session(string? status) => status switch
    {
        "Draft" => "Brouillon",
        "Open" => "Ouverte",
        "Closed" => "Fermee",
        "Archived" => "Archivee",
        _ => status ?? ""
    };

    public static string Need(string? status) => status switch
    {
        "Draft" => "Brouillon",
        "Submitted" => "Soumis",
        "UnderReview" => "En revision",
        "Approved" => "Approuve",
        "Rejected" => "Rejete",
        _ => status ?? ""
    };

    public static string BadgeClass(string? status) => status switch
    {
        "Open" or "Approved" => "badge good",
        "Submitted" or "UnderReview" or "Closed" => "badge warn",
        "Rejected" => "badge bad",
        "Archived" => "badge info",
        _ => "badge"
    };
}

public sealed class ApiErrorResponse
{
    public string? Error { get; set; }
    public string? Code { get; set; }
}

public sealed class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class RegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Role { get; set; }
}

public sealed class AuthResponse
{
    public string Token { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = "";
}

public sealed class MeResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
}

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public sealed class UpdateCurrentUserEmailRequest
{
    public string NewEmail { get; set; } = "";
    public string CurrentPassword { get; set; } = "";
}

public sealed class SessionResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public List<int>? CourseIds { get; set; }
}

public sealed class CreateSessionRequest
{
    public string Title { get; set; } = "";
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(4);
    public List<int>? CourseIds { get; set; }
    public int? CopyFromSessionId { get; set; }
}

public sealed class UpdateSessionRequest
{
    public string Title { get; set; } = "";
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(4);
}

public sealed class UpdateSessionCoursesRequest
{
    public List<int> CourseIds { get; set; } = [];
}

public sealed class CourseResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string? Name { get; set; }
}

public enum PersonnelFunction
{
    Professor = 1,
    LabInstructor = 2,
    CourseInstructor = 3
}

public static class PersonnelFunctions
{
    public static string Label(PersonnelFunction function) => function switch
    {
        PersonnelFunction.Professor => "Professeur(e)",
        PersonnelFunction.LabInstructor => "Charge(e) de laboratoire",
        PersonnelFunction.CourseInstructor => "Charge(e) de cours",
        _ => "Personnel"
    };
}

public sealed class PersonnelResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public PersonnelFunction Function { get; set; }
    public string Email { get; set; } = "";
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class CreateCourseRequest
{
    public string Code { get; set; } = "";
    public string? Name { get; set; }
}

public sealed class UpdateCourseRequest : CreateCourseRequest;

public sealed class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Roles { get; set; } = "";
    public bool IsActive { get; set; }
}

public sealed class CreateUserRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? RoleName { get; set; } = Roles.Professor;
}

public sealed class UpdateUserRoleRequest
{
    public string RoleName { get; set; } = Roles.Professor;
}

public sealed class UpdateUserPasswordRequest
{
    public string NewPassword { get; set; } = "";
}

public sealed class UserActivityResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string? FullName { get; set; }
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public List<UserTeachingNeedDetail> TeachingNeeds { get; set; } = [];
}

public sealed class UserTeachingNeedDetail
{
    public int Id { get; set; }
    public string CourseName { get; set; } = "";
    public string SessionName { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public int? ExpectedStudents { get; set; }
    public string? DesiredModifications { get; set; }
    public string? AdditionalComments { get; set; }
    public bool IsFastTrack { get; set; }
    public List<UserTeachingNeedItemDetail> Items { get; set; } = [];
}

public sealed class UserTeachingNeedItemDetail
{
    public int Id { get; set; }
    public string ItemType { get; set; } = "";
    public string? SoftwareName { get; set; }
    public string? VersionNumber { get; set; }
    public string? OsName { get; set; }
    public int? Quantity { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public sealed class TeachingNeedResponse
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int PersonnelId { get; set; }
    public string PersonnelFullName { get; set; } = "";
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = "";
    public string? CourseName { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public int? ExpectedStudents { get; set; }
    public bool? HasTechNeeds { get; set; }
    public bool? FoundAllCourses { get; set; }
    public string? DesiredModifications { get; set; }
    public bool? AllowsUpdates { get; set; }
    public string? AdditionalComments { get; set; }
    public List<TeachingNeedItemResponse> Items { get; set; } = [];
    public bool IsFastTrack { get; set; }
}

public sealed class TeachingNeedItemResponse
{
    public int Id { get; set; }
    public string ItemType { get; set; } = "";
    public int? SoftwareId { get; set; }
    public string? SoftwareName { get; set; }
    public int? SoftwareVersionId { get; set; }
    public string? SoftwareVersionNumber { get; set; }
    [JsonPropertyName("osId")]
    public int? OsId { get; set; }
    [JsonPropertyName("osName")]
    public string? OsName { get; set; }
    public int? Quantity { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? DetailsJson { get; set; }
    public bool? AlreadyInstalledInLabs { get; set; }
}

public class CreateTeachingNeedRequest
{
    public int CourseId { get; set; }
    public int? PersonnelId { get; set; }
    public string? Notes { get; set; }
    public int? ExpectedStudents { get; set; }
    public bool? HasTechNeeds { get; set; }
    public bool? FoundAllCourses { get; set; }
    public string? DesiredModifications { get; set; }
    public bool? AllowsUpdates { get; set; }
    public string? AdditionalComments { get; set; }
}

public sealed class UpdateTeachingNeedRequest : CreateTeachingNeedRequest;

public sealed class AddNeedItemRequest
{
    public string? ItemType { get; set; }
    public int? SoftwareId { get; set; }
    public int? SoftwareVersionId { get; set; }
    [JsonPropertyName("osId")]
    public int? OsId { get; set; }
    public int? Quantity { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? DetailsJson { get; set; }
}

public sealed class SubmitTeachingNeedResponse
{
    public TeachingNeedResponse Need { get; set; } = new();
    public List<string> Warnings { get; set; } = [];
}

public sealed class RejectTeachingNeedRequest
{
    public string Reason { get; set; } = "";
}

public sealed class MyNeedResponse
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string SessionTitle { get; set; } = "";
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = "";
    public string? CourseName { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
}

public sealed class RenewableCourseResponse
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = "";
    public string? CourseName { get; set; }
    public int SourceNeedId { get; set; }
    public int SourceSessionId { get; set; }
    public string SourceSessionTitle { get; set; } = "";
    public int ItemCount { get; set; }
}

public sealed class RenewNeedsResponse
{
    public TeachingNeedResponse Need { get; set; } = new();
    public List<string> Changes { get; set; } = [];
}

public sealed class RenewAllResponse
{
    public List<RenewNeedsResponse> Renewed { get; set; } = [];
    public int TotalCourses { get; set; }
    public int TotalItems { get; set; }
}

public sealed class SaaSProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int? NumberOfAccounts { get; set; }
    public string? Notes { get; set; }
}

public class CreateSaaSProductRequest
{
    public string Name { get; set; } = "";
    public int? NumberOfAccounts { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateSaaSProductRequest : CreateSaaSProductRequest;

public sealed class SoftwareResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? InstallCommand { get; set; }
    public List<SoftwareVersionResponse>? SoftwareVersions { get; set; }
}

public class CreateSoftwareRequest
{
    public string Name { get; set; } = "";
}

public sealed class UpdateSoftwareRequest : CreateSoftwareRequest;

public sealed class SoftwareVersionResponse
{
    public int Id { get; set; }
    public int SoftwareId { get; set; }
    [JsonPropertyName("osId")]
    public int OsId { get; set; }
    public string VersionNumber { get; set; } = "";
    public string? InstallationDetails { get; set; }
    public string? Notes { get; set; }
}

public sealed class CreateSoftwareVersionRequest
{
    public int SoftwareId { get; set; }
    [JsonPropertyName("osId")]
    public int OsId { get; set; }
    public string VersionNumber { get; set; } = "";
    public string? InstallationDetails { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateSoftwareVersionRequest
{
    [JsonPropertyName("osId")]
    public int OsId { get; set; }
    public string VersionNumber { get; set; } = "";
    public string? InstallationDetails { get; set; }
    public string? Notes { get; set; }
}

public sealed class SoftwareCatalogEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? InstallCommand { get; set; }
    public List<SoftwareVersionCatalogEntry> Versions { get; set; } = [];
}

public sealed class SoftwareVersionCatalogEntry
{
    public int Id { get; set; }
    public string VersionNumber { get; set; } = "";
    [JsonPropertyName("osId")]
    public int OsId { get; set; }
    [JsonPropertyName("osName")]
    public string OsName { get; set; } = "";
    public string? InstallationDetails { get; set; }
    public string? Notes { get; set; }
}

public sealed class ConfigurationResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    [JsonPropertyName("osIds")]
    public List<int> OsIds { get; set; } = [];
    public List<int> LaboratoryIds { get; set; } = [];
    public string? Notes { get; set; }
}

public class CreateConfigurationRequest
{
    public string Title { get; set; } = "";
    [JsonPropertyName("osIds")]
    public List<int> OsIds { get; set; } = [];
    public List<int> LaboratoryIds { get; set; } = [];
    public string? Notes { get; set; }
}

public sealed class UpdateConfigurationRequest : CreateConfigurationRequest;

public sealed class VirtualMachineResponse
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public int CpuCores { get; set; }
    public int RamGb { get; set; }
    public int StorageGb { get; set; }
    public string AccessType { get; set; } = "";
    public string? Notes { get; set; }
    [JsonPropertyName("osId")]
    public int OsId { get; set; }
    [JsonPropertyName("osName")]
    public string OsName { get; set; } = "";
    public int? HostServerId { get; set; }
    public string? HostServerHostname { get; set; }
}

public class CreateVirtualMachineRequest
{
    public int Quantity { get; set; } = 1;
    public int CpuCores { get; set; } = 1;
    public int RamGb { get; set; } = 1;
    public int StorageGb { get; set; } = 20;
    public string AccessType { get; set; } = "";
    public string? Notes { get; set; }
    [JsonPropertyName("osId")]
    public int OsId { get; set; }
    public int? HostServerId { get; set; }
}

public sealed class UpdateVirtualMachineRequest : CreateVirtualMachineRequest;

public sealed class PhysicalServerResponse
{
    public int Id { get; set; }
    public string Hostname { get; set; } = "";
    public int CpuCores { get; set; }
    public int RamGb { get; set; }
    public int StorageGb { get; set; }
    public string AccessType { get; set; } = "";
    public string? Notes { get; set; }
    [JsonPropertyName("osId")]
    public int OsId { get; set; }
    [JsonPropertyName("osName")]
    public string OsName { get; set; } = "";
}

public class CreatePhysicalServerRequest
{
    public string Hostname { get; set; } = "";
    public int CpuCores { get; set; } = 1;
    public int RamGb { get; set; } = 1;
    public int StorageGb { get; set; } = 20;
    public string AccessType { get; set; } = "";
    public string? Notes { get; set; }
    [JsonPropertyName("osId")]
    public int OsId { get; set; }
}

public sealed class UpdatePhysicalServerRequest : CreatePhysicalServerRequest;

public sealed class EquipmentModelResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public string? DefaultAccessories { get; set; }
    public string? Notes { get; set; }
}

public class CreateEquipmentModelRequest
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public string? DefaultAccessories { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateEquipmentModelRequest : CreateEquipmentModelRequest;

public sealed class OSResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<SoftwareVersionResponse>? SoftwareVersions { get; set; }
}

public sealed class LaboratoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Building { get; set; } = "";
    [JsonPropertyName("numberOfPCs")]
    public int NumberOfPCs { get; set; }
    public int SeatingCapacity { get; set; }
    public List<WorkstationResponse> Workstations { get; set; } = [];
}

public sealed class WorkstationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int LaboratoryId { get; set; }
    [JsonPropertyName("osId")]
    public int OsId { get; set; }
    [JsonPropertyName("osName")]
    public string OsName { get; set; } = "";
}

public sealed class LaboratorySoftwareResponse
{
    public int LaboratoryId { get; set; }
    public string LaboratoryName { get; set; } = "";
    public int SoftwareId { get; set; }
    public string SoftwareName { get; set; } = "";
    public string Status { get; set; } = "";
}

public sealed class UpsertLaboratorySoftwareRequest
{
    public string Status { get; set; } = "";
}

public sealed class CourseSaaSResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int? NumberOfAccounts { get; set; }
    public string? Notes { get; set; }
}

public sealed class CourseSoftwareResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? InstallCommand { get; set; }
}

public sealed class CourseConfigurationResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    [JsonPropertyName("osIds")]
    public List<int>? OsIds { get; set; }
    public List<int>? LaboratoryIds { get; set; }
    public string? Notes { get; set; }
}

public sealed class CourseVmResponse
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public int CpuCores { get; set; }
    public int RamGb { get; set; }
    public int StorageGb { get; set; }
    public string AccessType { get; set; } = "";
    [JsonPropertyName("osName")]
    public string OsName { get; set; } = "";
    public string? HostServerHostname { get; set; }
    public string? Notes { get; set; }
}

public sealed class CourseServerResponse
{
    public int Id { get; set; }
    public string Hostname { get; set; } = "";
    public int CpuCores { get; set; }
    public int RamGb { get; set; }
    public int StorageGb { get; set; }
    public string AccessType { get; set; } = "";
    [JsonPropertyName("osName")]
    public string OsName { get; set; } = "";
    public string? Notes { get; set; }
}

public sealed class CourseEquipmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public string? DefaultAccessories { get; set; }
    public string? Notes { get; set; }
}

public sealed class CourseResourcesResponse
{
    [JsonPropertyName("saaS")]
    public List<CourseSaaSResponse> SaaS { get; set; } = [];
    public List<CourseSoftwareResponse> Softwares { get; set; } = [];
    public List<CourseConfigurationResponse> Configurations { get; set; } = [];
    public List<CourseVmResponse> VirtualMachines { get; set; } = [];
    public List<CourseServerResponse> PhysicalServers { get; set; } = [];
    public List<CourseEquipmentResponse> Equipment { get; set; } = [];
    public List<int> SoftwareVersionIds { get; set; } = [];
}

public sealed class AiStatusResponse
{
    public bool Available { get; set; }
}

public sealed class AiSuggestRequest
{
    public int SessionId { get; set; }
    public int CourseId { get; set; }
    public string? ItemType { get; set; }
}

public sealed class AiSuggestedItem
{
    public string ItemType { get; set; } = "";
    public string Label { get; set; } = "";
    public string? SoftwareName { get; set; }
    public string? Version { get; set; }
    public string? Os { get; set; }
    public string? InstallCommand { get; set; }
    public string? Notes { get; set; }
    public string Reason { get; set; } = "";
}

public sealed class AiSuggestResponse
{
    public List<AiSuggestedItem> Suggestions { get; set; } = [];
    public string? Summary { get; set; }
}

public sealed class AiAnalyzeRequest
{
    public int SessionId { get; set; }
    public int NeedId { get; set; }
}

public sealed class AiReviewAnalysis
{
    public string Summary { get; set; } = "";
    public List<string> Alerts { get; set; } = [];
    public string? SuggestedAction { get; set; }
    public string? DraftRejectReason { get; set; }
    public List<AiHistoryComparison> HistoryComparisons { get; set; } = [];
}

public sealed class AiHistoryComparison
{
    public string SessionTitle { get; set; } = "";
    public string Similarity { get; set; } = "";
}

public sealed class RejectionAssistRequest
{
    public int SessionId { get; set; }
    public int NeedId { get; set; }
}

public sealed class RejectionAssistResponse
{
    public string Explanation { get; set; } = "";
    public List<CorrectionStep> Steps { get; set; } = [];
    public string? RevisedNotes { get; set; }
}

public sealed class CorrectionStep
{
    public string Action { get; set; } = "";
    public string Target { get; set; } = "";
    public string Detail { get; set; } = "";
}

public sealed class AutoFillRequest
{
    public int SessionId { get; set; }
    public int CourseId { get; set; }
    public string ItemType { get; set; } = "";
    public Dictionary<string, string> CurrentValues { get; set; } = [];
}

public sealed class AutoFillResponse
{
    public Dictionary<string, AutoFillSuggestion> Suggestions { get; set; } = [];
    public string Source { get; set; } = "";
}

public sealed class AutoFillSuggestion
{
    public string Value { get; set; } = "";
    public string Reason { get; set; } = "";
    public float Confidence { get; set; }
}
