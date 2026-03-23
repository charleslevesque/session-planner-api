using SessionPlanner.Api.Dtos.EquipmentModels;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.EquipmentModels;

public sealed class CreateEquipmentModelRequestExample : IExamplesProvider<CreateEquipmentModelRequest>
{
    public CreateEquipmentModelRequest GetExamples()
    {
        return new CreateEquipmentModelRequest(
            Name: "Playstation 5 Controller",
            Quantity: 15,
            DefaultAccessories: "Power adapter",
            Notes: "Standard controller"
        );
    }
}