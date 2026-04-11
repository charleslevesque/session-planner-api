using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Entities.Joins;
using SessionPlanner.Core.Enums;

namespace SessionPlanner.Core.Entities;

public class Session
{
    private static readonly Dictionary<SessionStatus, SessionStatus> AllowedTransitions = new()
    {
        { SessionStatus.Draft, SessionStatus.Open },
        { SessionStatus.Open, SessionStatus.Closed },
        { SessionStatus.Closed, SessionStatus.Archived },
    };

    public int Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public SessionStatus Status { get; set; } = SessionStatus.Draft;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<TeachingNeed> TeachingNeeds { get; set; } = new List<TeachingNeed>();
    public ICollection<SessionCourse> SessionCourses { get; set; } = new List<SessionCourse>();

    public void TransitionTo(SessionStatus target)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || allowed != target)
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to {target}.");

        Status = target;

        switch (target)
        {
            case SessionStatus.Open:
                OpenedAt = DateTime.UtcNow;
                break;
            case SessionStatus.Closed:
                ClosedAt = DateTime.UtcNow;
                break;
            case SessionStatus.Archived:
                ArchivedAt = DateTime.UtcNow;
                break;
        }
    }
}
