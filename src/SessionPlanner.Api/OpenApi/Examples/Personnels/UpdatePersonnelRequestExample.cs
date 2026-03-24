using SessionPlanner.Api.Dtos.Personnel;
using SessionPlanner.Core.Entities;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Personnel;

public sealed class UpdatePersonnelRequestExample : IExamplesProvider<UpdatePersonnelRequest>
{
    public UpdatePersonnelRequest GetExamples()
    {
        return new UpdatePersonnelRequest(
            FirstName: "Jean",
            LastName: "Douin",
            Function: PersonnelFunction.LabInstructor,
            Email: "jean.douin@example.ca"
        );
    }
}