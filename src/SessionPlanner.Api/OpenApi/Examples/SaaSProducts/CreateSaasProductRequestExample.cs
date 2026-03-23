using SessionPlanner.Api.Dtos.SaaSProducts;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.SaaSProducts;

public sealed class CreateSaaSProductRequestExample : IExamplesProvider<CreateSaaSProductRequest>
{
    public CreateSaaSProductRequest GetExamples()
    {
        return new CreateSaaSProductRequest(
            Name: "Slack",
            NumberOfAccounts: 120,
            Notes: "Licensed accounts for teaching and administrative staff."
        );
    }
}