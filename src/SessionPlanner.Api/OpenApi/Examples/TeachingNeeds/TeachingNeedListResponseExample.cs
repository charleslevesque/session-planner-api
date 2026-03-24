using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.TeachingNeeds;
using SessionPlanner.Core.Enums;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class TeachingNeedListExample : IExamplesProvider<IEnumerable<TeachingNeedResponse>>
{
    public IEnumerable<TeachingNeedResponse> GetExamples()
    {
        return new List<TeachingNeedResponse>
        {
            new TeachingNeedResponse(
                Id: 42,
                SessionId: 5,
                PersonnelId: 12,
                PersonnelFullName: "Anthony Dumais",
                CourseId: 3,
                CourseCode: "GTI350",
                CourseName: "Conception et evaluation des interfaces utilisateurs",
                CreatedAt: DateTime.Now,
                Status: "Draft",
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
                Items: new List<TeachingNeedItemResponse>()
            ),
            new TeachingNeedResponse(
                Id: 52,
                SessionId: 3,
                PersonnelId: 10,
                PersonnelFullName: "Jean Morin",
                CourseId: 2,
                CourseCode: "LOG320",
                CourseName: "Structures de donnees et algorithmes",
                CreatedAt: DateTime.Now,
                Status: "Submitted",
                SubmittedAt: DateTime.Now.AddDays(2),
                ReviewedAt: null,
                ReviewedByUserId: null,
                RejectionReason: null,
                ExpectedStudents: 30,
                HasTechNeeds: false,
                FoundAllCourses: false,
                DesiredModifications: "",
                AdditionalComments: "Has to be available before the first week of the semester",
                AllowsUpdates: false,
                Notes: "Need software for programming",
                Items: new List<TeachingNeedItemResponse>()
            )
        };
    }
}