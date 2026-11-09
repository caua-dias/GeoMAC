namespace GeoMac.Application.Configuration;

public sealed class LlmSettings
{
    public const string SectionName = "LlmSettings";

    public string BaseUrl { get; set; } = "http://localhost:1235/v1";
    public string ModelId { get; set; } = "qwen2.5-7b-instruct";
    public int TimeoutSeconds { get; set; } = 120;
    public double Temperature { get; set; } = 0.2;
}
