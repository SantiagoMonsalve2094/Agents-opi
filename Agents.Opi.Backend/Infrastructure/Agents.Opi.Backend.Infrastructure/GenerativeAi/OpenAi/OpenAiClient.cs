using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Agents.Opi.Backend.Application.DTOs.Agent;
using Agents.Opi.Backend.Application.Exceptions;
using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Domain.Enums;
using Agents.Opi.Backend.Infrastructure.Resources;
using Microsoft.Extensions.Options;

namespace Agents.Opi.Backend.Infrastructure.GenerativeAi.OpenAi;

public sealed class OpenAiClient(HttpClient httpClient, IOptions<OpenAiOptions> options) : IAgentGenerationPort
{
    private const string HekateApiKeyEnvironmentVariable = "HEKATE_OPENAI__APIKEY";
    private const string DefaultApiKeyEnvironmentVariable = "OPENAI__APIKEY";
    private const string RuntimeSyllabusFilePattern = "*ISTQB*CTFL*V4*.runtime.md";
    private const string SyllabusFilePattern = "*ISTQB*CTFL*V4*.md";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly OpenAiOptions _options = options.Value;

    public async Task<string> GenerateAsync(GenerativeAiRequest request, CancellationToken cancellationToken)
    {
        var apiKey = GetApiKey(request.AgentType);
        var requestBody = await CreateRequestBodyAsync(request, stream: false, cancellationToken);
        using var httpRequest = CreateRequest(requestBody, apiKey);
        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new GenerativeAiException(string.Format(InfrastructureMessages.OpenAiError, (int)response.StatusCode), responseText);
        }

        var output = ExtractOutputText(responseText);
        return string.IsNullOrWhiteSpace(output)
            ? InfrastructureMessages.OpenAiEmptyOutput
            : output;
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(
        GenerativeAiRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var apiKey = GetApiKey(request.AgentType);
        var requestBody = await CreateRequestBodyAsync(request, stream: true, cancellationToken);
        using var httpRequest = CreateRequest(requestBody, apiKey);
        using var response = await httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new GenerativeAiException(string.Format(InfrastructureMessages.OpenAiError, (int)response.StatusCode), error);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var emittedText = false;

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var json = line["data:".Length..].Trim();
            if (json == "[DONE]")
            {
                yield break;
            }

            var text = ExtractText(json, emittedText);
            if (!string.IsNullOrEmpty(text))
            {
                emittedText = true;
                yield return text;
            }
        }
    }

    private static string GetApiKey(AgentType agentType)
    {
        var variableName = agentType == AgentType.QA
            ? HekateApiKeyEnvironmentVariable
            : DefaultApiKeyEnvironmentVariable;

        var apiKey = Environment.GetEnvironmentVariable(variableName) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(InfrastructureMessages.MissingOpenAiApiKey);
        }

        return apiKey;
    }

    private static HttpRequestMessage CreateRequest(OpenAiResponsesRequest body, string apiKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(body, options: JsonOptions);

        return request;
    }

    private async Task<OpenAiResponsesRequest> CreateRequestBodyAsync(
        GenerativeAiRequest request,
        bool stream,
        CancellationToken cancellationToken)
    {
        var content = new List<OpenAiInputContent>();
        if (request.IncludeKnowledgeSource)
        {
            var syllabus = await GetSyllabusFileAsync(cancellationToken);
            content.Add(new OpenAiInputContent("input_text", syllabus, null, null, null));
        }

        content.Add(new OpenAiInputContent("input_text", request.Prompt, null, null, null));

        return new OpenAiResponsesRequest(
            _options.Model,
            [new OpenAiInputMessage("user", content)],
            GetTemperatureForModel(_options.Model),
            _options.MaxOutputTokens,
            stream);
    }

    private static double? GetTemperatureForModel(string model)
        => model.StartsWith("gpt-5", StringComparison.OrdinalIgnoreCase) ? null : 0.1;

    private static async Task<string> GetSyllabusFileAsync(CancellationToken cancellationToken)
    {
        var path = FindSyllabusPath();
        var content = await File.ReadAllTextAsync(path, cancellationToken);
        return $"SYLLABUS ISTQB CTFL V4.0 ({Path.GetFileName(path)}):\n\n{content}";
    }

    private static string FindSyllabusPath()
    {
        foreach (var startPoint in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(startPoint);
            while (directory is not null)
            {
                var knowledgeSourceDirectory = Path.Combine(directory.FullName, "KnowledgeSources", "ISTQB");
                var candidate = Directory.Exists(knowledgeSourceDirectory)
                    ? FindSyllabusCandidate(knowledgeSourceDirectory)
                    : null;

                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return Path.GetFullPath(candidate);
                }

                candidate = FindSyllabusCandidate(directory.FullName);
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return Path.GetFullPath(candidate);
                }

                directory = directory.Parent;
            }
        }

        throw new InvalidOperationException(InfrastructureMessages.SyllabusMarkdownNotFound);
    }

    private static string? FindSyllabusCandidate(string directory)
        => Directory.EnumerateFiles(directory, RuntimeSyllabusFilePattern, SearchOption.TopDirectoryOnly).FirstOrDefault()
            ?? Directory
                .EnumerateFiles(directory, SyllabusFilePattern, SearchOption.TopDirectoryOnly)
                .Where(file => !file.EndsWith(".runtime.md", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

    private static string ExtractText(string json, bool emittedText)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
        {
            return string.Empty;
        }

        var type = typeElement.GetString();
        if (string.Equals(type, "response.output_text.delta", StringComparison.OrdinalIgnoreCase) &&
            root.TryGetProperty("delta", out var deltaElement) &&
            deltaElement.ValueKind == JsonValueKind.String)
        {
            return deltaElement.GetString() ?? string.Empty;
        }

        if (string.Equals(type, "error", StringComparison.OrdinalIgnoreCase) &&
            root.TryGetProperty("error", out var errorElement))
        {
            throw new GenerativeAiException(InfrastructureMessages.OpenAiStreamError, ExtractOpenAiError(errorElement));
        }

        if (string.Equals(type, "response.failed", StringComparison.OrdinalIgnoreCase) &&
            root.TryGetProperty("response", out var failedResponseElement) &&
            failedResponseElement.TryGetProperty("error", out var failedErrorElement))
        {
            throw new GenerativeAiException(InfrastructureMessages.OpenAiStreamError, ExtractOpenAiError(failedErrorElement));
        }

        if (string.Equals(type, "response.incomplete", StringComparison.OrdinalIgnoreCase))
        {
            throw new GenerativeAiException(InfrastructureMessages.OpenAiStreamError, ExtractIncompleteReason(root));
        }

        if (string.Equals(type, "response.completed", StringComparison.OrdinalIgnoreCase) &&
            root.TryGetProperty("response", out var completedResponseElement) &&
            IsIncompleteResponse(completedResponseElement))
        {
            throw new GenerativeAiException(InfrastructureMessages.OpenAiStreamError, ExtractIncompleteReason(completedResponseElement));
        }

        if (emittedText)
        {
            return string.Empty;
        }

        if (string.Equals(type, "response.output_text.done", StringComparison.OrdinalIgnoreCase) &&
            root.TryGetProperty("text", out var textElement) &&
            textElement.ValueKind == JsonValueKind.String)
        {
            return textElement.GetString() ?? string.Empty;
        }

        if (string.Equals(type, "response.completed", StringComparison.OrdinalIgnoreCase) &&
            root.TryGetProperty("response", out var responseElement) &&
            responseElement.ValueKind == JsonValueKind.Object)
        {
            return ExtractOutputText(responseElement.GetRawText());
        }

        return string.Empty;
    }

    private static bool IsIncompleteResponse(JsonElement responseElement)
        => responseElement.ValueKind == JsonValueKind.Object &&
           responseElement.TryGetProperty("status", out var statusElement) &&
           statusElement.ValueKind == JsonValueKind.String &&
           string.Equals(statusElement.GetString(), "incomplete", StringComparison.OrdinalIgnoreCase);

    private static string ExtractIncompleteReason(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty("incomplete_details", out var detailsElement) &&
            detailsElement.ValueKind == JsonValueKind.Object &&
            detailsElement.TryGetProperty("reason", out var reasonElement) &&
            reasonElement.ValueKind == JsonValueKind.String)
        {
            return $"{InfrastructureMessages.OpenAiIncompleteOutput} Motivo: {reasonElement.GetString()}.";
        }

        return InfrastructureMessages.OpenAiIncompleteOutput;
    }

    private static string ExtractOpenAiError(JsonElement errorElement)
    {
        if (errorElement.ValueKind == JsonValueKind.Object &&
            errorElement.TryGetProperty("message", out var messageElement) &&
            messageElement.ValueKind == JsonValueKind.String)
        {
            return messageElement.GetString() ?? InfrastructureMessages.OpenAiUnknownError;
        }

        return errorElement.GetRawText();
    }

    private static string ExtractOutputText(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.TryGetProperty("output_text", out var outputTextElement) &&
            outputTextElement.ValueKind == JsonValueKind.String)
        {
            return outputTextElement.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("output", out var outputElement) || outputElement.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var parts = new List<string>();
        foreach (var outputItem in outputElement.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var contentElement) ||
                contentElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in contentElement.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var textElement) &&
                    textElement.ValueKind == JsonValueKind.String)
                {
                    parts.Add(textElement.GetString() ?? string.Empty);
                }
            }
        }

        return string.Concat(parts);
    }

    private sealed record OpenAiResponsesRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] IReadOnlyList<OpenAiInputMessage> Input,
        [property: JsonPropertyName("temperature")] double? Temperature,
        [property: JsonPropertyName("max_output_tokens")] int MaxOutputTokens,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record OpenAiInputMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] IReadOnlyList<OpenAiInputContent> Content);

    private sealed record OpenAiInputContent(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("text")] string? Text,
        [property: JsonPropertyName("file_id")] string? FileId,
        [property: JsonPropertyName("filename")] string? FileName,
        [property: JsonPropertyName("file_data")] string? FileData);
}
