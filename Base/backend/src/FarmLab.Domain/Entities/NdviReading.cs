using System;

namespace FarmLab.Domain.Entities
{
    public class NdviReading
    {
        public int Id { get; set; }
        public int TalhaoId { get; set; }
        public DateTime Date { get; set; }
        // valores estatísticos do NDVI
        public double Min { get; set; }
        public double Max { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double StdDev { get; set; }
        // Percentuais de classes de vegetação
        public double VegLowPct { get; set; }
        public double VegMediumPct { get; set; }
        public double VegHighPct { get; set; }
        public double VegDensePct { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
