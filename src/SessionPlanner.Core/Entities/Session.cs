using System.ComponentModel.DataAnnotations;
using SessionPlanner.Core.Enums;

namespace SessionPlanner.Core.Entities;

public class Session
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public SessionStatus Status { get; set; } = SessionStatus.Draft;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TeachingNeed> TeachingNeeds { get; set; } = new List<TeachingNeed>();
}
