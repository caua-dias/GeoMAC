using System;

namespace GeoMac.Domain.Entities;

public class NdviReading
{
    public Guid Id { get; set; }
    public Guid CropAnalysisId { get; set; }
    public DateTime Date { get; set; }
    public double NdviValue { get; set; }
}
