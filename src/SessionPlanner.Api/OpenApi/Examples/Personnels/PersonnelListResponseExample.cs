using SessionPlanner.Api.Dtos.Personnel;
using SessionPlanner.Core.Entities;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Personnel;

public sealed class PersonnelListResponseExample : IExamplesProvider<IEnumerable<PersonnelResponse>>
{
    public IEnumerable<PersonnelResponse> GetExamples()
    {
        return
        [
            new PersonnelResponse(
                Id: 1,
                FirstName: "Jean",
                LastName: "Douin",
                Function: PersonnelFunction.Professor,
                Email: "jean.douin@example.ca"
            ),
            new PersonnelResponse(
                Id: 2,
                FirstName: "Alex",
                LastName: "Desjardins",
                Function: PersonnelFunction.CourseInstructor,
                Email: "alex.desjardins@example.ca"
            )
        ];
    }
}