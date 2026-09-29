// GeoMac.Domain/Entities/WeatherMetric.cs
using System;

namespace GeoMac.Domain.Entities
{
    public class WeatherMetric
    {
        public Guid Id { get; set; }
        public Guid CropAnalysisId { get; set; }
        public DateTime Date { get; set; }
        public double PrecipitationMm { get; set; }
        public double TemperatureC { get; set; }
    }
}
