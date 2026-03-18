using SessionPlanner.Core.Enums;

namespace SessionPlanner.Core.Entities;

public class TeachingNeed
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public int PersonnelId { get; set; }
    public Personnel Personnel { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public NeedStatus Status { get; set; } = NeedStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }

    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }

    public int? ExpectedStudents { get; set; }
    public bool? HasTechNeeds { get; set; }
    public bool? FoundAllCourses { get; set; }
    public string? DesiredModifications { get; set; }
    public bool? AllowsUpdates { get; set; }
    public string? AdditionalComments { get; set; }

    public ICollection<TeachingNeedItem> Items { get; set; } = new List<TeachingNeedItem>();
}