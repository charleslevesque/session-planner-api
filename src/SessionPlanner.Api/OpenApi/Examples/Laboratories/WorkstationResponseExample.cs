using SessionPlanner.Api.Dtos.Workstations;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.Laboratories;

public sealed class WorkstationResponseExample : IExamplesProvider<WorkstationResponse>
{
    public WorkstationResponse GetExamples()
    {
        return new WorkstationResponse(
            Id: 1,
            Name: "WS-B2042-01",
            LaboratoryId: 1,
            OSId: 2,
            OSName: "Windows 11"
        );
    }
}