using System.Text.Json;
using GeoMac.Application.DTOs.Llm;
using GeoMac.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeoMac.Presentation.Controllers;

[ApiController]
[Route("api/assistant")]
[Produces("application/json")]
public sealed class AgronomicAssistantController(ILlmService llmService) : ControllerBase
{
    private readonly ILlmService _llmService = llmService;

    /// <summary>
    /// Gera um diagnóstico agronômico estruturado em JSON a partir das métricas de NDVI, solo e clima usando Qwen 2.5 7B.
    /// </summary>
    [HttpPost("diagnose")]
    [ProducesResponseType(typeof(AgronomicDiagnosisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> GenerateDiagnosis(
        [FromBody] AgronomicDiagnosisRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FieldName))
            return BadRequest(new { message = "O nome do talhão (FieldName) é obrigatório." });

        try
        {
            var diagnosis = await _llmService.GenerateAgronomicDiagnosisAsync(request, cancellationToken);
            return Ok(diagnosis);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { message = "Tempo limite excedido ao aguardar resposta do servidor LM Studio." });
        }
        catch (JsonException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "A LLM não retornou um JSON estruturado válido.", detail = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Falha de comunicação com o servidor LM Studio.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Envia uma pergunta técnica livre para o Engenheiro Agrônomo Virtual (Qwen 2.5 7B Instruct).
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { message = "A mensagem não pode estar vazia." });

        try
        {
            var response = await _llmService.ChatWithAgronomistAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { message = "Tempo limite excedido ao aguardar resposta do servidor LM Studio." });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Falha de comunicação com o servidor LM Studio.", detail = ex.Message });
        }
    }
}
