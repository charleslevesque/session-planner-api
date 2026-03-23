using SessionPlanner.Api.Dtos.Laboratories;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Laboratories;

public sealed class LaboratoryListResponseExample : IExamplesProvider<IEnumerable<LaboratoryResponse>>
{
    public IEnumerable<LaboratoryResponse> GetExamples()
    {
        return
        [
            new LaboratoryResponse(
                Id: 1,
                Name: "B-2042",
                Building: "B",
                NumberOfPCs: 26,
                SeatingCapacity: 24,
                Workstations: []
            ),
            new LaboratoryResponse(
                Id: 2,
                Name: "A-1302",
                Building: "A",
                NumberOfPCs: 30,
                SeatingCapacity: 30,
                Workstations: []
            )
        ];
    }
}