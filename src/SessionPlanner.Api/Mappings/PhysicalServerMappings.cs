using SessionPlanner.Api.Dtos.PhysicalServers;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class PhysicalServerMappings
{
    public static PhysicalServerResponse ToResponse(this PhysicalServer server)
    {
        return new PhysicalServerResponse(
            server.Id,
            server.Hostname,
            server.CpuCores,
            server.RamGb,
            server.StorageGb,
            server.AccessType,
            server.Notes,
            server.OSId,
            server.OS?.Name ?? string.Empty
        );
    }
}
