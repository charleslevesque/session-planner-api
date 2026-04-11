using SessionPlanner.Core.Entities;
using SessionPlanner.Api.Dtos.TeachingNeeds;

namespace SessionPlanner.Api.Mappings;

public static class TeachingNeedMappings
{
    public static TeachingNeedItemResponse ToResponse(this TeachingNeedItem item,
        IReadOnlyDictionary<int, bool>? installedMap = null) => new(
        item.Id,
        item.ItemType,
        item.SoftwareId,
        item.Software?.Name,
        item.SoftwareVersionId,
        item.SoftwareVersion?.VersionNumber,
        item.OSId,
        item.OS?.Name,
        item.Quantity,
        item.Description,
        item.Notes,
        item.DetailsJson,
        installedMap is not null && installedMap.TryGetValue(item.Id, out var installed) ? installed : null);

    public static TeachingNeedResponse ToResponse(this TeachingNeed need,
        IReadOnlyDictionary<int, bool>? installedMap = null) => new(
        need.Id,
        need.SessionId,
        need.PersonnelId,
        $"{need.Personnel?.FirstName} {need.Personnel?.LastName}".Trim(),
        need.CourseId,
        need.Course?.Code ?? string.Empty,
        need.Course?.Name,
        need.Status.ToString(),
        need.CreatedAt,
        need.SubmittedAt,
        need.ReviewedAt,
        need.ReviewedByUserId,
        need.RejectionReason,
        need.Notes,
        need.ExpectedStudents,
        need.HasTechNeeds,
        need.FoundAllCourses,
        need.DesiredModifications,
        need.AllowsUpdates,
        need.AdditionalComments,
        need.Items.Select(i => i.ToResponse(installedMap)),
        need.IsFastTrack);

    public static MyNeedResponse ToMyNeedResponse(this TeachingNeed need) => new(
        need.Id,
        need.SessionId,
        need.Session?.Title ?? string.Empty,
        need.CourseId,
        need.Course?.Code ?? string.Empty,
        need.Course?.Name,
        need.Status.ToString(),
        need.CreatedAt,
        need.SubmittedAt,
        need.ReviewedAt,
        need.RejectionReason,
        need.Notes);
}
