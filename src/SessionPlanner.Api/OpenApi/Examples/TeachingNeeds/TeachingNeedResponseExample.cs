using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.TeachingNeeds;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class TeachingNeedResponseExample : IExamplesProvider<TeachingNeedResponse>
{
    public TeachingNeedResponse GetExamples()
    {
        return new TeachingNeedResponse(
            Id: 42,
            SessionId: 5,
            PersonnelId: 12,
            PersonnelFullName: "Anthony Dumais",
            CourseId: 3,
            CourseCode: "GTI350",
            CourseName: "Conception et evaluation des interfaces utilisateurs",
            CreatedAt: DateTime.Now,
            Status: "Submitted",
            SubmittedAt: null,
            ReviewedAt: null,
            ReviewedByUserId: null,
            RejectionReason: null,
            Notes: "Need software for API testing",
            Items: new List<TeachingNeedItemResponse>
            {
                new TeachingNeedItemResponse(
                    Id: 1,
                    SoftwareId: 3,
                    SoftwareName: "Postman",
                    SoftwareVersionId: 25,
                    SoftwareVersionNumber: "10.2.19",
                    OSId: 2,
                    OSName: "Linux",
                    Quantity: 30,
                    Notes: "Installed on all lab machines"
                )
            }
        );
    }
}