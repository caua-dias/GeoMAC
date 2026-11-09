namespace GeoMac.Application.DTOs.Llm;

public sealed class AgronomicDiagnosisResponseDto
{
    public string ExecutiveSummary { get; set; } = string.Empty;
    public double YieldLossProbability { get; set; }
    public string OverallRiskLevel { get; set; } = string.Empty;
    public List<LimitingFactorDto> LimitingFactors { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
}

public sealed class LimitingFactorDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}
