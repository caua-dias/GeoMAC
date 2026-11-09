using GeoMac.Application.Configuration;
using GeoMac.Application.DTOs.Llm;
using GeoMac.Application.Interfaces;
using GeoMac.Infrastructure;
using GeoMac.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GeoMac.Tests.Presentation;

public class AgronomicAssistantControllerAndDiTests
{
    [Fact]
    public void InjetorDeDependenciasDeveVincularLlmSettingsAPartirDoConfigurationComSucesso()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["LlmSettings:BaseUrl"] = "http://localhost:1235/v1",
            ["LlmSettings:ModelId"] = "qwen2.5-7b-instruct",
            ["LlmSettings:TimeoutSeconds"] = "90",
            ["LlmSettings:Temperature"] = "0.3",
            ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<LlmSettings>>().Value;
        var llmService = serviceProvider.GetService<ILlmService>();

        // Assert
        Assert.Equal("http://localhost:1235/v1", options.BaseUrl);
        Assert.Equal("qwen2.5-7b-instruct", options.ModelId);
        Assert.Equal(90, options.TimeoutSeconds);
        Assert.Equal(0.3, options.Temperature);
        Assert.NotNull(llmService);
    }

    [Fact]
    public async Task ControllerGenerateDiagnosisDeveRetornarOkComObjetoAgronomicDiagnosisResponseDto()
    {
        // Arrange
        var fakeLlmService = new FakeLlmService();
        var controller = new AgronomicAssistantController(fakeLlmService);

        var request = new AgronomicDiagnosisRequestDto
        {
            FieldName = "Grão 4.0",
            CropName = "Milho",
            PeakNdvi = 0.82,
            FinalNdvi = 0.50,
            MinSoilPh = 4.65,
            ExchangeableAluminum = 1.48,
            ReproductiveStagePrecipitationMm = 0.0,
            MaxTemperatureCelsius = 35.9
        };

        // Act
        var actionResult = await controller.GenerateDiagnosis(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var responseDto = Assert.IsType<AgronomicDiagnosisResponseDto>(okResult.Value);
        Assert.Equal("Critical", responseDto.OverallRiskLevel);
    }

    [Fact]
    public async Task ControllerChatDeveRetornarBadRequestQuandoMensagemEstiverVazia()
    {
        // Arrange
        var fakeLlmService = new FakeLlmService();
        var controller = new AgronomicAssistantController(fakeLlmService);

        // Act
        var actionResult = await controller.Chat(new ChatRequestDto { Message = "   " }, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Fact]
    public async Task ControllerChatDeveRetornarOkComRespostaDoAgronomoQuandoMensagemForValida()
    {
        // Arrange
        var fakeLlmService = new FakeLlmService();
        var controller = new AgronomicAssistantController(fakeLlmService);

        // Act
        var actionResult = await controller.Chat(
            new ChatRequestDto { Message = "Qual a dose ideal de calcário?" },
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var responseDto = Assert.IsType<ChatResponseDto>(okResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(responseDto.Reply));
    }

    private sealed class FakeLlmService : ILlmService
    {
        public Task<AgronomicDiagnosisResponseDto> GenerateAgronomicDiagnosisAsync(
            AgronomicDiagnosisRequestDto request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AgronomicDiagnosisResponseDto
            {
                ExecutiveSummary = "Diagnóstico simulado com sucesso.",
                YieldLossProbability = 0.80,
                OverallRiskLevel = "Critical",
                LimitingFactors =
                [
                    new LimitingFactorDto
                    {
                        Category = "Edafológico",
                        Description = "Alumínio elevado",
                        Severity = "Critical"
                    }
                ],
                Recommendations = ["Calagem em taxa variável"]
            });
        }

        public Task<ChatResponseDto> ChatWithAgronomistAsync(
            ChatRequestDto request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ChatResponseDto
            {
                Reply = $"Resposta técnica para: {request.Message}",
                Model = "qwen2.5-7b-instruct"
            });
        }
    }
}
