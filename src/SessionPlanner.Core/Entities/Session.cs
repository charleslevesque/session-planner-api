namespace SessionPlanner.Core.Entities;

public class Session
{
    public int Id {get; set;}
    public string Title {get; set;} = string.Empty;
    public DateTime Date {get; set;}

    public ICollection<TeachingNeed> TeachingNeeds { get; set; } = new List<TeachingNeed>();
}