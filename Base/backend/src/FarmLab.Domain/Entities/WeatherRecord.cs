using System;

namespace FarmLab.Domain.Entities
{
    public class WeatherRecord
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public double SolarRadiation { get; set; }
        public double Precipitation { get; set; }
        public double WindSpeed { get; set; }
        public double TemperatureMin { get; set; }
        public double TemperatureAvg { get; set; }
        public double TemperatureMax { get; set; }
        public double HumidityMin { get; set; }
        public double HumidityAvg { get; set; }
        public double HumidityMax { get; set; }
        public double Evapotranspiration { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
