using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record RejectTeachingNeedRequest(
    [Required] string Reason);
