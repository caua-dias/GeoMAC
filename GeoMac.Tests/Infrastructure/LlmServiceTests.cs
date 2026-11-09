using System.Net;
using System.Text;
using System.Text.Json;
using GeoMac.Application.Configuration;
using GeoMac.Application.DTOs.Llm;
using GeoMac.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace GeoMac.Tests.Infrastructure;

public class LlmServiceTests
{
    private static IOptions<LlmSettings> CriarConfiguracoesPadrao() =>
        Options.Create(new LlmSettings
        {
            BaseUrl = "http://localhost:1235/v1",
            ModelId = "qwen2.5-7b-instruct",
            TimeoutSeconds = 120,
            Temperature = 0.2
        });

    [Fact]
    public async Task LlmServiceDeveChamarEndpointChatCompletionsComModeloQwenCorreto()
    {
        // Arrange
        Uri? capturedUri = null;
        string? capturedRequestBody = null;

        var fakeResponseJson = """
            {
              "choices": [
                {
                  "message": {
                    "role": "assistant",
                    "content": "Recomendo realizar calagem e gessagem a taxa variável."
                  }
                }
              ]
            }
            """;

        var handler = new FakeHttpMessageHandler((request, _) =>
        {
            capturedUri = request.RequestUri;
            capturedRequestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fakeResponseJson, Encoding.UTF8, "application/json")
            });
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:1235/v1/") };
        var service = new LlmService(httpClient, CriarConfiguracoesPadrao());

        // Act
        var response = await service.ChatWithAgronomistAsync(new ChatRequestDto
        {
            Message = "Como corrigir acidez e alumínio no solo arenoso?",
            TalhaoContext = "Grão 4.0 - pH 4.65"
        });

        // Assert
        Assert.NotNull(capturedUri);
        Assert.EndsWith("/v1/chat/completions", capturedUri.AbsolutePath);
        Assert.NotNull(capturedRequestBody);
        Assert.Contains("qwen2.5-7b-instruct", capturedRequestBody);
        Assert.Contains("RTX 4060 Ti", capturedRequestBody);
        Assert.Contains("Cropman", capturedRequestBody);
        Assert.Equal("Recomendo realizar calagem e gessagem a taxa variável.", response.Reply);
    }

    [Fact]
    public async Task GenerateAgronomicDiagnosisAsyncDeveRetornarDiagnosticoEstruturadoMesmoQuandoLlmEnvolverEmMarkdown()
    {
        // Arrange
        var llmMarkdownContent = """
            ```json
            {
              "executiveSummary": "Colapso de NDVI de 0.827 para 0.507 devido a estresse hídrico e térmico.",
              "yieldLossProbability": 0.75,
              "overallRiskLevel": "Critical",
              "limitingFactors": [
                {
                  "category": "Solo",
                  "description": "pH 4.65 e Alumínio em 1.48 cmol/dm³",
                  "severity": "Critical"
                }
              ],
              "recommendations": [
                "Aplicar calcário e gesso a taxa variável",
                "Introduzir Urochloa na entressafra"
              ]
            }
            ```
            """;

        var openAiEnvelope = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        role = "assistant",
                        content = llmMarkdownContent
                    }
                }
            }
        });

        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(openAiEnvelope, Encoding.UTF8, "application/json")
            }));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:1235/v1/") };
        var service = new LlmService(httpClient, CriarConfiguracoesPadrao());

        var requestDto = new AgronomicDiagnosisRequestDto
        {
            FieldName = "Grão 4.0",
            CropName = "Milho DKB358PRO4",
            PeakNdvi = 0.827,
            FinalNdvi = 0.507,
            MinSoilPh = 4.65,
            ExchangeableAluminum = 1.48,
            ReproductiveStagePrecipitationMm = 0.0,
            MaxTemperatureCelsius = 35.9
        };

        // Act
        var diagnosis = await service.GenerateAgronomicDiagnosisAsync(requestDto);

        // Assert
        Assert.NotNull(diagnosis);
        Assert.Equal("Critical", diagnosis.OverallRiskLevel);
        Assert.Equal(0.75, diagnosis.YieldLossProbability);
        Assert.Single(diagnosis.LimitingFactors);
        Assert.Equal(2, diagnosis.Recommendations.Count);
    }

    [Fact]
    public async Task LlmServiceDeveRespeitarCancellationTokenQuandoRequisicaoForCancelada()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:1235/v1/") };
        var service = new LlmService(httpClient, CriarConfiguracoesPadrao());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ChatWithAgronomistAsync(new ChatRequestDto { Message = "Teste de cancelamento" }, cts.Token));
    }

    [Fact]
    public async Task LlmServiceDeveLancarHttpRequestExceptionQuandoServidorLmStudioRetornarErro500()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("LM Studio model not loaded", Encoding.UTF8, "text/plain")
            }));

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:1235/v1/") };
        var service = new LlmService(httpClient, CriarConfiguracoesPadrao());

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.ChatWithAgronomistAsync(new ChatRequestDto { Message = "Olá" }));
    }

    private sealed class FakeHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            handlerFunc(request, cancellationToken);
    }
}
