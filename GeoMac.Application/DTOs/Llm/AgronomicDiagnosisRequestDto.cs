namespace GeoMac.Application.DTOs.Llm;

public sealed class AgronomicDiagnosisRequestDto
{
    public string FieldName { get; set; } = string.Empty;
    public string CropName { get; set; } = string.Empty;
    public double PeakNdvi { get; set; }
    public double FinalNdvi { get; set; }
    public double MinSoilPh { get; set; }
    public double ExchangeableAluminum { get; set; }
    public double ReproductiveStagePrecipitationMm { get; set; }
    public double MaxTemperatureCelsius { get; set; }
    public string? AdditionalNotes { get; set; }
}
