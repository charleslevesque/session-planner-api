namespace SessionPlanner.Api.Dtos.TeachingNeeds;

public record SubmitTeachingNeedResponse(
    TeachingNeedResponse Need,
    IEnumerable<string> Warnings
);
