using SessionPlanner.Core.Entities.Joins;
namespace SessionPlanner.Core.Entities;

public class SaaSProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;        // Ex: "ABAP S/4HANA"
    public int? NumberOfAccounts { get; set; }       // Column "Nombre de comptes"
    public string? Notes { get; set; }               // Ex: "UCC sap.cob.csuchico.edu"

    public ICollection<CourseSaaSProduct> CourseSaaSProducts { get; set; } = new List<CourseSaaSProduct>();
}