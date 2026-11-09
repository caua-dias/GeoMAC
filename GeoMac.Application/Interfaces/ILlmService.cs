using GeoMac.Application.DTOs.Llm;

namespace GeoMac.Application.Interfaces;

public interface ILlmService
{
    Task<AgronomicDiagnosisResponseDto> GenerateAgronomicDiagnosisAsync(
        AgronomicDiagnosisRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ChatResponseDto> ChatWithAgronomistAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default);
}
