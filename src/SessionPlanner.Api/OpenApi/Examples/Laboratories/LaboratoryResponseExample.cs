using SessionPlanner.Api.Dtos.Laboratories;
using SessionPlanner.Api.Dtos.Workstations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Laboratories;

public sealed class LaboratoryResponseExample : IExamplesProvider<LaboratoryResponse>
{
    public LaboratoryResponse GetExamples()
    {
        return new LaboratoryResponse(
            Id: 1,
            Name: "B-2042",
            Building: "B",
            NumberOfPCs: 26,
            SeatingCapacity: 24,
            Workstations:
            [
                new WorkstationResponse(
                    Id: 1,
                    LaboratoryId: 1,
                    Name: "WS-B2042-01",
                    OSId: 2,
                    OSName: "Windows 11"
                )
            ]
        );
    }
}