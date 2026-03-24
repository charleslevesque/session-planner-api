using SessionPlanner.Api.Dtos.EquipmentModels;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.EquipmentModels;

public sealed class UpdateEquipmentModelRequestExample : IExamplesProvider<UpdateEquipmentModelRequest>
{
    public UpdateEquipmentModelRequest GetExamples()
    {
        return new UpdateEquipmentModelRequest(
            Name: "Playstation 5 Controller",
            Quantity: 26,
            DefaultAccessories: "Power adapter",
            Notes: "added new stock to inventory"
        );
    }
}