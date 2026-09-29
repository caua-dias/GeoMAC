using System;
using System.Collections.Generic;

namespace GeoMac.Domain.Entities
{
    public class CropAnalysis
    {
        public Guid Id { get; set; }
        public string CropName { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public ICollection<WeatherMetric> WeatherMetrics { get; set; } = new List<WeatherMetric>();
        public ICollection<SoilMetric> SoilMetrics { get; set; } = new List<SoilMetric>();
        public ICollection<NdviReading> NdviReadings { get; set; } = new List<NdviReading>();
        public ICollection<ActionPlan> ActionPlans { get; set; } = new List<ActionPlan>();
    }
}
