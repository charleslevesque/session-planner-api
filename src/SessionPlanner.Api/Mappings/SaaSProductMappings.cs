using SessionPlanner.Api.Dtos.SaaSProducts;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Api.Mappings;

public static class SaaSProductMappings
{
    public static SaaSProductResponse ToResponse(this SaaSProduct product)
    {
        return new SaaSProductResponse(
            product.Id,
            product.Name,
            product.NumberOfAccounts,
            product.Notes
        );
    }
}
