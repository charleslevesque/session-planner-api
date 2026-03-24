using SessionPlanner.Api.Dtos.EquipmentModels;
using Swashbuckle.AspNetCore.Filters;

namespace SessionPlanner.Api.OpenApi.Examples.EquipmentModels;

public sealed class EquipmentModelResponseExample : IExamplesProvider<EquipmentModelResponse>
{
    public EquipmentModelResponse GetExamples()
    {
        return new EquipmentModelResponse(
            Id: 1,
            Name: "Playstation 5 Controller",
            Quantity: 15,
            DefaultAccessories: "Power adapter",
            Notes: "Standard controller"
        );
    }
}