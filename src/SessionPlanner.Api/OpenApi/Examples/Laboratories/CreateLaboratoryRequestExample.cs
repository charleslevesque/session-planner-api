using SessionPlanner.Api.Dtos.Laboratories;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Laboratories;

public sealed class CreateLaboratoryRequestExample : IExamplesProvider<CreateLaboratoryRequest>
{
    public CreateLaboratoryRequest GetExamples()
    {
        return new CreateLaboratoryRequest(
            Name: "B-2042",
            Building: "B",
            NumberOfPCs: 26,
            SeatingCapacity: 24
        );
    }
}