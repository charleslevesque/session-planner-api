using SessionPlanner.Api.Dtos.SaaSProducts;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.SaaSProducts;

public sealed class SaaSProductResponseExample : IExamplesProvider<SaaSProductResponse>
{
    public SaaSProductResponse GetExamples()
    {
        return new SaaSProductResponse(
            Id: 1,
            Name: "Slack",
            NumberOfAccounts: 120,
            Notes: "Licensed accounts for teaching and administrative staff."
        );
    }
}