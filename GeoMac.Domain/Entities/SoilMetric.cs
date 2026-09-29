using System;

namespace GeoMac.Domain.Entities;

public class SoilMetric
{
    public Guid Id { get; set; }
    public Guid CropAnalysisId { get; set; }
    public DateTime Date { get; set; }
    public double PH { get; set; }
    public double AluminumMgPerKg { get; set; }
    public double OrganicMatterPercentage { get; set; }
}
