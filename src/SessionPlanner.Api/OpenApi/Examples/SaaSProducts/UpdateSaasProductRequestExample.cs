using SessionPlanner.Api.Dtos.SaaSProducts;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.SaaSProducts;

public sealed class UpdateSaaSProductRequestExample : IExamplesProvider<UpdateSaaSProductRequest>
{
    public UpdateSaaSProductRequest GetExamples()
    {
        return new UpdateSaaSProductRequest(
            Name: "Slack",
            NumberOfAccounts: 150,
            Notes: "Licensed accounts for teaching and administrative staff.\nAdded new accounts."
        );
    }
}