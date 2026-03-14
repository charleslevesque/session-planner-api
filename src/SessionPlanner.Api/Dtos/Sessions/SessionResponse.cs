using SessionPlanner.Core.Enums;

namespace SessionPlanner.Api.Dtos.Sessions;

public record SessionResponse(
    int Id,
    string Title,
    SessionStatus Status,
    DateTime StartDate,
    DateTime EndDate,
    DateTime CreatedAt,
    DateTime? OpenedAt,
    DateTime? ClosedAt,
    DateTime? ArchivedAt,
    int? CreatedByUserId
);
