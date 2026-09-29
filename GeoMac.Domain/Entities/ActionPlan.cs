using System;

namespace GeoMac.Domain.Entities;

public class ActionPlan
{
    public Guid Id { get; set; }
    public Guid CropAnalysisId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
