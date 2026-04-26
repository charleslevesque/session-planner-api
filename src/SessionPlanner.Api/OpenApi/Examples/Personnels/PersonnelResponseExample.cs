using SessionPlanner.Api.Dtos.Personnel;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Personnel;

public sealed class PersonnelResponseExample : IExamplesProvider<PersonnelResponse>
{
    public PersonnelResponse GetExamples()
    {
        return new PersonnelResponse(
            Id: 1,
            FirstName: "Jean",
            LastName: "Douin",
            Function: PersonnelFunction.Professor,
            Email: "jean.douin@example.ca"
        );
    }
}