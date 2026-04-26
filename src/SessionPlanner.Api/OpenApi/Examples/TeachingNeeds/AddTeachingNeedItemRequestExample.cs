using Swashbuckle.AspNetCore.Filters;
using SessionPlanner.Api.Dtos.TeachingNeeds;
using SessionPlanner.Core.Enums;

namespace SessionPlanner.Api.OpenApi.Examples.TeachingNeeds;

public sealed class AddTeachingNeedItemRequestExample : IExamplesProvider<AddNeedItemRequest>
{
    public AddNeedItemRequest GetExamples()
    {
        return new AddNeedItemRequest(
            ItemType: NeedItemType.Software,
            Description: "License pour la session",
            SoftwareId: 10,
            SoftwareVersionId: 25,
            OSId: 2,
            Quantity: 30,
            Notes: "Before the 3rd week of the semester",
            DetailsJson: "{\"name\":\"Example\"}"
        );
    }
}