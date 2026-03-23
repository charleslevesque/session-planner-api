using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.TeachingNeeds;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class AddTeachingNeedItemRequestExample : IExamplesProvider<AddNeedItemRequest>
{
    public AddNeedItemRequest GetExamples()
    {
        return new AddNeedItemRequest(
            SoftwareId: 10,
            SoftwareVersionId: 25,
            OSId: 2,
            Quantity: 30,
            Notes: "Before the 3rd week of the semester"
        );
    }
}