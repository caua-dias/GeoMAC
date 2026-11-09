namespace GeoMac.Application.DTOs.Llm;

public sealed class ChatRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string? TalhaoContext { get; set; }
}

public sealed class ChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
