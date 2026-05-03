namespace SessionPlanner.Web.Models;

public record SessionResponse(int Id, string Title, string Status, string StartDate, string EndDate, string CreatedAt);
public record SessionCourseResponse(int Id, string Code, string? Name);
public record CreateSessionRequest(string Title, string StartDate, string EndDate);
