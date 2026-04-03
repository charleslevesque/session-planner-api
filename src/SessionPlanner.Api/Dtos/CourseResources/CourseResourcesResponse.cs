namespace SessionPlanner.Api.Dtos.CourseResources;

public record CourseResourcesResponse(
    IReadOnlyList<CourseSaaSResponse> SaaS,
    IReadOnlyList<CourseSoftwareResponse> Softwares,
    IReadOnlyList<CourseConfigurationResponse> Configurations,
    IReadOnlyList<CourseVmResponse> VirtualMachines,
    IReadOnlyList<CourseServerResponse> PhysicalServers,
    IReadOnlyList<CourseEquipmentResponse> Equipment,
    IReadOnlyList<int> SoftwareVersionIds
);
