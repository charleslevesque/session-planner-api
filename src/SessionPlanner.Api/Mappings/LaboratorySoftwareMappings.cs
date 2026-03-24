using SessionPlanner.Api.Dtos.LaboratorySoftwares;
using SessionPlanner.Core.Entities.Joins;

namespace SessionPlanner.Api.Mappings;

public static class LaboratorySoftwareMappings
{
    public static LaboratorySoftwareResponse ToResponse(this LaboratorySoftware ls)
        => new(
            ls.LaboratoryId,
            ls.Laboratory.Name,
            ls.SoftwareId,
            ls.Software.Name,
            ls.Status
        );
}
