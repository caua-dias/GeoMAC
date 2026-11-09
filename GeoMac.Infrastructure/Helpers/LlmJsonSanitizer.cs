using System.Text.Json;
using System.Text.RegularExpressions;

namespace GeoMac.Infrastructure.Helpers;

public static partial class LlmJsonSanitizer
{
    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    [GeneratedRegex(@"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex MarkdownCodeBlockRegex();

    public static string ExtractCleanJson(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return string.Empty;

        var trimmed = rawInput.Trim();

        // 1. Verifica se há bloco de código markdown ```json ... ``` ou ``` ... ```
        var match = MarkdownCodeBlockRegex().Match(trimmed);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }

        // 2. Caso não haja backticks, extrai do primeiro '{' ao último '}'
        var firstBrace = trimmed.IndexOf('{');
        var lastBrace = trimmed.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return trimmed[firstBrace..(lastBrace + 1)];
        }

        return trimmed;
    }

    public static T DeserializeSafe<T>(string rawInput)
    {
        var cleanJson = ExtractCleanJson(rawInput);

        if (string.IsNullOrWhiteSpace(cleanJson))
            throw new JsonException("A resposta da LLM está vazia ou não contém um payload JSON válido.");

        return JsonSerializer.Deserialize<T>(cleanJson, DefaultJsonOptions)
               ?? throw new JsonException($"Falha ao desserializar o JSON para o tipo {typeof(T).Name}.");
    }
}
