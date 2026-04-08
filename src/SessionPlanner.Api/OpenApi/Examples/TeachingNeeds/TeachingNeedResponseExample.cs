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
            ExpectedStudents: 40,
            HasTechNeeds: false,
            FoundAllCourses: false,
            DesiredModifications: "I need the Pro License this semester",
            AdditionalComments: "",
            AllowsUpdates: false,
            Notes: "Need software for API testing",
            Items: new List<TeachingNeedItemResponse>
            {
                new TeachingNeedItemResponse(
                    Id: 1,
                    ItemType: "Item type example",
                    Description: "License pour la session",
                    SoftwareId: 10,
                    SoftwareVersionId: 25,
                    SoftwareVersionNumber: "2022.1.4",
                    SoftwareName: "Photoshop CC",
                    OSId: 2,
                    OSName: "Windows",
                    Quantity: 30,
                    Notes: "Installed on all machines",
                    DetailsJson: "{\"name\":\"Example\"}",
                    AlreadyInstalledInLabs: true
                )
            }
        );
    }
}