using System.Text.Json;
using GeoMac.Application.DTOs.Llm;
using GeoMac.Infrastructure.Helpers;
using Xunit;

namespace GeoMac.Tests.Infrastructure;

public class LlmJsonSanitizerTests
{
    [Fact]
    public void SanitizadorJsonDeveRemoverBlocosMarkdownComTagJson()
    {
        // Arrange
        var rawOutput = """
            Aqui está a análise solicitada:
            ```json
            {
              "executiveSummary": "Quebra severa por déficit hídrico e alumínio tóxico.",
              "yieldLossProbability": 0.78,
              "overallRiskLevel": "Critical"
            }
            ```
            Espero ter ajudado!
            """;

        // Act
        var cleanJson = LlmJsonSanitizer.ExtractCleanJson(rawOutput);

        // Assert
        Assert.StartsWith("{", cleanJson);
        Assert.EndsWith("}", cleanJson);
        Assert.DoesNotContain("```", cleanJson);
        Assert.Contains("\"executiveSummary\"", cleanJson);
    }

    [Fact]
    public void SanitizadorJsonDeveRemoverBlocosMarkdownGenericosSemTagDeLinguagem()
    {
        // Arrange
        var rawOutput = """
            ```
            {
              "executiveSummary": "Estresse térmico em VT/R1.",
              "yieldLossProbability": 0.65,
              "overallRiskLevel": "High"
            }
            ```
            """;

        // Act
        var cleanJson = LlmJsonSanitizer.ExtractCleanJson(rawOutput);

        // Assert
        Assert.StartsWith("{", cleanJson);
        Assert.EndsWith("}", cleanJson);
        Assert.DoesNotContain("```", cleanJson);
    }

    [Fact]
    public void SanitizadorJsonDeveManterJsonPuroInalterado()
    {
        // Arrange
        var rawJson = """{"executiveSummary":"Solo saudável","yieldLossProbability":0.05,"overallRiskLevel":"Low"}""";

        // Act
        var cleanJson = LlmJsonSanitizer.ExtractCleanJson(rawJson);

        // Assert
        Assert.Equal(rawJson, cleanJson);
    }

    [Fact]
    public void DesserializadorJsonDeveConverterParaAgronomicDiagnosisResponseDtoIgnorandoCase()
    {
        // Arrange
        var markdownJson = """
            ```json
            {
              "EXECUTIVESUMMARY": "Déficit hídrico no florescimento.",
              "YIELDLOSSPROBABILITY": 0.85,
              "OVERALLRISKLEVEL": "Critical",
              "LIMITINGFACTORS": [
                {
                  "category": "Agrometeorológico",
                  "description": "Temperatura acima de 35.9°C",
                  "severity": "High"
                }
              ],
              "RECOMMENDATIONS": [
                "Correção a taxa variável com calcário e gesso."
              ]
            }
            ```
            """;

        // Act
        var result = LlmJsonSanitizer.DeserializeSafe<AgronomicDiagnosisResponseDto>(markdownJson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Déficit hídrico no florescimento.", result.ExecutiveSummary);
        Assert.Equal(0.85, result.YieldLossProbability);
        Assert.Equal("Critical", result.OverallRiskLevel);
        Assert.Single(result.LimitingFactors);
        Assert.Equal("Agrometeorológico", result.LimitingFactors[0].Category);
        Assert.Single(result.Recommendations);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Texto sem nenhum objeto json valido")]
    public void DesserializadorJsonDeveLancarJsonExceptionQuandoConteudoForInvalidoOuVazio(string invalidInput)
    {
        // Act & Assert
        Assert.ThrowsAny<JsonException>(() =>
            LlmJsonSanitizer.DeserializeSafe<AgronomicDiagnosisResponseDto>(invalidInput));
    }
}
