using System.ComponentModel.DataAnnotations;

namespace SessionPlanner.Core.Entities;

public class TeachingNeedItem
{
    public int Id { get; set; }

    public int TeachingNeedId { get; set; }
    public TeachingNeed TeachingNeed { get; set; } = null!;

    [MaxLength(30)]
    public string ItemType { get; set; } = "software";

    public int? SoftwareId { get; set; }
    public Software? Software { get; set; }

    public int? SoftwareVersionId { get; set; }
    public SoftwareVersion? SoftwareVersion { get; set; }

    public int? OSId { get; set; }
    public OS? OS { get; set; }

    public int? Quantity { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? DetailsJson { get; set; }
}