using SessionPlanner.Api.Dtos.EquipmentModels;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.EquipmentModels;

public sealed class EquipmentModelListResponseExample : IExamplesProvider<IEnumerable<EquipmentModelResponse>>
{
    public IEnumerable<EquipmentModelResponse> GetExamples()
    {
        return
        [
            new EquipmentModelResponse(
                Id: 1,
                Name: "Playstation 5 Controller",
                Quantity: 15,
                DefaultAccessories: "Power adapter",
                Notes: "Standard controller"
            ),
            new EquipmentModelResponse(
                Id: 2,
                Name: "Meta Quest 3",
                Quantity: 5,
                DefaultAccessories: "Power cable, controller",
                Notes: ""
            )
        ];
    }
}