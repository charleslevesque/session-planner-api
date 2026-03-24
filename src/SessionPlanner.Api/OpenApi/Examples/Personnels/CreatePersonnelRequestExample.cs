using SessionPlanner.Api.Dtos.Personnel;
using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.OpenApi.Examples.Personnel;

public sealed class CreatePersonnelRequestExample : IExamplesProvider<CreatePersonnelRequest>
{
    public CreatePersonnelRequest GetExamples()
    {
        return new CreatePersonnelRequest(
            FirstName: "Jean",
            LastName: "Douin",
            Function: PersonnelFunction.Professor,
            Email: "jean.douin@example.ca"
        );
    }
}