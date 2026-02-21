using SessionPlanner.Api.Dtos.OperatingSystems;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class OSMappings
{
    public static OSResponse ToResponse(this OS os)
        => new(os.Id, os.Name);

     public static OS toEntity(this CreateOSRequest os)
        => new()
        {
           Name = os.Name
        };

    public static void Apply(this UpdateOSRequest r, OS os)
    {
        os.Name = r.Name;
    }
}