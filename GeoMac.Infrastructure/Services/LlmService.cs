using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GeoMac.Application.Configuration;
using GeoMac.Application.DTOs.Llm;
using GeoMac.Application.Interfaces;
using GeoMac.Infrastructure.Helpers;
using Microsoft.Extensions.Options;

namespace GeoMac.Infrastructure.Services;

public sealed class LlmService : ILlmService
{
    public const string BasePersonaPrompt = """
        # Identidade e Papel
        Você é o motor de Inteligência Agronômica do GeoMAC (FarmLab). Sua função dupla consiste em gerar diagnósticos automáticos precisos de quebras de safra e atuar como um chatbot consultivo técnico para produtores e agrônomos.

        # Contexto Arquitetural e de Infraestrutura
        - Modelo em Execução: Qwen 2.5 7B Instruct (Q4_K_M).
        - Hardware Host: PC dedicado com GPU RTX 4060 Ti (8 GB VRAM), processamento inferencial 100% alocado na GPU via LM Studio.
        - Comunicação de Rede: O tráfego entre o servidor da aplicação (Acer Aspire 3) e o servidor de inferência ocorre via malha VPN local utilizando o Radmin VPN.
        - Integração de Software: Você é consumido exclusivamente via API REST (padrão OpenAI) pelo backend estruturado em .NET 10 (Clean Architecture).

        # Domínio de Conhecimento e Análise
        Seus diagnósticos devem correlacionar sinergicamente as bases de dados do GeoMAC:
        1. Limitações Edafológicas (Cropman): Textura do solo, pH, toxidez por alumínio e saturação por bases.
        2. Riscos Agrometeorológicos (Metos): Déficit hídrico, radiação e picos de estresse térmico em janelas fenológicas críticas (ex: pendoamento VT/R1).
        3. Dinâmica Espaço-Temporal (OneSoil): Curvas de índice vegetativo (NDVI) para rastrear biomassa, vigor e senescência precoce.
        4. Falhas Operacionais (EKOS): Telemetria de maquinário e apontamentos de OS (aplicações de defensivos e manejo).

        # Diretrizes de Saída
        1. Chamadas via API (.NET Backend - Diagnóstico Automático): Quando solicitado pelo sistema, responda ESTRITAMENTE em formato JSON validado, estruturando os dados para desserialização direta nos objetos do C# (ex: `CropAnalysisDto`, `ActionPlanDto`). Não inclua textos explicativos fora da estrutura JSON.
        2. Interações via Chatbot (Consulta Humana): Responda em português fluente, adotando um tom profissional, analítico e fundamentado em literatura agronômica.
        """;

    private readonly HttpClient _httpClient;
    private readonly LlmSettings _settings;

    public LlmService(HttpClient httpClient, IOptions<LlmSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;

        if (_httpClient.BaseAddress is null && !string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            var baseUrl = _settings.BaseUrl.EndsWith('/')
                ? _settings.BaseUrl
                : $"{_settings.BaseUrl}/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<AgronomicDiagnosisResponseDto> GenerateAgronomicDiagnosisAsync(
        AgronomicDiagnosisRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = $$"""
            {{BasePersonaPrompt}}

            [MODO ATIVO: 1. Chamada via API (.NET Backend - Diagnóstico Automático)]
            Responda ESTRITAMENTE com um objeto JSON válido seguindo exatamente este schema:
            {
              "executiveSummary": "Resumo técnico correlacionando solo (Cropman), clima (Metos), NDVI (OneSoil) e operação (EKOS)",
              "yieldLossProbability": 0.0,
              "overallRiskLevel": "Low | Medium | High | Critical",
              "limitingFactors": [
                {
                  "category": "Edafológico | Agrometeorológico | Espaço-Temporal | Operacional",
                  "description": "Descrição técnica detalhada do fator limitante",
                  "severity": "Low | Medium | High | Critical"
                }
              ],
              "recommendations": [
                "Proposição de manejo 1",
                "Proposição de manejo 2"
              ]
            }
            """;

        var userPrompt = $"""
            Realize o diagnóstico agronômico automático para o seguinte talhão:
            - Talhão: {request.FieldName}
            - Cultura / Híbrido: {request.CropName}
            - NDVI de Pico (OneSoil): {request.PeakNdvi:F3}
            - NDVI Final Pré-colheita (OneSoil): {request.FinalNdvi:F3}
            - pH Mínimo do Solo em CaCl2 (Cropman): {request.MinSoilPh:F2}
            - Alumínio Trocável em cmol/dm³ (Cropman): {request.ExchangeableAluminum:F2}
            - Precipitação no Estádio Reprodutivo VT/R1 (Metos): {request.ReproductiveStagePrecipitationMm:F1} mm
            - Temperatura Máxima Registrada (Metos): {request.MaxTemperatureCelsius:F1} °C
            - Observações Operacionais e de Campo (EKOS): {request.AdditionalNotes ?? "Nenhuma"}
            """;

        var rawReply = await SendChatCompletionAsync(systemPrompt, userPrompt, cancellationToken);
        return LlmJsonSanitizer.DeserializeSafe<AgronomicDiagnosisResponseDto>(rawReply);
    }

    public async Task<ChatResponseDto> ChatWithAgronomistAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = $"""
            {BasePersonaPrompt}

            [MODO ATIVO: 2. Interação via Chatbot (Consulta Humana)]
            """;

        var userPrompt = string.IsNullOrWhiteSpace(request.TalhaoContext)
            ? request.Message
            : $"[Contexto do Talhão: {request.TalhaoContext}]\n\nPergunta: {request.Message}";

        var rawReply = await SendChatCompletionAsync(systemPrompt, userPrompt, cancellationToken);

        return new ChatResponseDto
        {
            Reply = rawReply.Trim(),
            Model = _settings.ModelId
        };
    }

    private async Task<string> SendChatCompletionAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        var payload = new OpenAiChatRequest
        {
            Model = _settings.ModelId,
            Temperature = _settings.Temperature,
            Messages =
            [
                new OpenAiChatMessage { Role = "system", Content = systemPrompt },
                new OpenAiChatMessage { Role = "user", Content = userPrompt }
            ]
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "chat/completions",
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var completion = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>(
            cancellationToken: cancellationToken);

        var content = completion?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("O modelo LLM retornou uma resposta vazia.");

        return content;
    }

    private sealed class OpenAiChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("messages")]
        public List<OpenAiChatMessage> Messages { get; set; } = [];
    }

    private sealed class OpenAiChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private sealed class OpenAiChatResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAiChoice>? Choices { get; set; }
    }

    private sealed class OpenAiChoice
    {
        [JsonPropertyName("message")]
        public OpenAiChatMessage? Message { get; set; }
    }
}
