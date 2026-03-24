using SessionPlanner.Api.Dtos.Laboratories;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Laboratories;

public sealed class UpdateLaboratoryRequestExample : IExamplesProvider<UpdateLaboratoryRequest>
{
    public UpdateLaboratoryRequest GetExamples()
    {
        return new UpdateLaboratoryRequest(
            Name: "C-1025 (new update)",
            Building: "C",
            NumberOfPCs: 20,
            SeatingCapacity: 18
        );
    }
}