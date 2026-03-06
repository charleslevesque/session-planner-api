using SessionPlanner.Api.Dtos.EquipmentModels;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class EquipmentModelMappings
{
    public static EquipmentModelResponse ToResponse(this EquipmentModel equipment)
    {
        return new EquipmentModelResponse(
            equipment.Id,
            equipment.Name,
            equipment.Quantity,
            equipment.DefaultAccessories,
            equipment.Notes
        );
    }
}
