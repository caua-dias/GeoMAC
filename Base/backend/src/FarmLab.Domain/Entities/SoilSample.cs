using System;

namespace FarmLab.Domain.Entities
{
    public class SoilSample
    {
        public int Id { get; set; }
        public int TalhaoId { get; set; }
        public DateTime SampleDate { get; set; }
        // Atributos típicos de análise de solo (exemplo subset)
        public double PH { get; set; }
        public double Aluminum { get; set; }
        public double OrganicMatter { get; set; }
        public double Phosphorus { get; set; }
        public double CTC { get; set; }
        public double SaturationBases { get; set; }
        public double Clay { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
