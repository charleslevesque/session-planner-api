using SessionPlanner.Api.Dtos.SaaSProducts;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.SaaSProducts;

public sealed class SaaSProductListResponseExample : IExamplesProvider<IEnumerable<SaaSProductResponse>>
{
    public IEnumerable<SaaSProductResponse> GetExamples()
    {
        return
        [
            new SaaSProductResponse(
                Id: 1,
                Name: "Slack",
                NumberOfAccounts: 120,
                Notes: "Licensed accounts for teaching and administrative staff."
            ),
            new SaaSProductResponse(
                Id: 2,
                Name: "Dropbox",
                NumberOfAccounts: 40,
                Notes: "Licenses for shared storage space"
            )
        ];
    }
}