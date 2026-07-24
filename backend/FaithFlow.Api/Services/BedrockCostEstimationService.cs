using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using FaithFlow.Backend.Common;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using Microsoft.Extensions.Options;

namespace FaithFlow.Backend.Services;

/// <summary>
/// Produces structured cost estimates by prompting a Claude model on Amazon Bedrock.
/// </summary>
public class BedrockCostEstimationService : ICostEstimationService
{
    private readonly IAmazonBedrockRuntime _bedrock;
    private readonly BedrockSettings _settings;
    private readonly ILogger<BedrockCostEstimationService> _logger;

    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private const string SystemPrompt = """
        You are a cost estimation assistant for an organizational request-tracking system.
        Users submit requests of many kinds — for example Supply, Travel, Services, IT,
        Facilities, Labor, and Prayer/pastoral care — and you produce a realistic cost
        estimate in US Dollars for fulfilling the request.

        Guidelines:
        - Base your estimate on typical current US market prices for the described item or service.
        - Consider the request type as important context (e.g. "Travel" implies transport/lodging,
          "IT" implies hardware/software/licenses, "Supply" implies physical goods).
        - If a request has no meaningful monetary cost (e.g. a prayer or well-wishes request),
          return 0 for all three estimates, confidence "high", and say so briefly in the reasoning.
        - Provide a plausible range: low_estimate <= most_likely <= high_estimate.
        - Set "confidence" based on how much detail the request provides:
          "high" when the item/service and scope are clear, "medium" when partially specified,
          "low" when the description is vague or ambiguous.
        - Keep "reasoning" to one or two concise sentences.

        Respond with ONLY a single minified JSON object and no other text, using EXACTLY this schema:
        {"low_estimate": <number>, "most_likely": <number>, "high_estimate": <number>, "currency": "USD", "confidence": "low" | "medium" | "high", "reasoning": "<short explanation>"}
        Do not wrap the JSON in markdown code fences.
        """;

    public BedrockCostEstimationService(
        IAmazonBedrockRuntime bedrock,
        IOptions<BedrockSettings> settings,
        ILogger<BedrockCostEstimationService> logger)
    {
        _bedrock = bedrock;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<CostEstimateResponseDto> EstimateAsync(
        CostEstimateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Description))
        {
            throw new CostEstimationException("A title or description is required to estimate cost.");
        }

        var userPrompt =
            $"Request type: {request.RequestType}\n" +
            $"Title: {request.Title}\n" +
            $"Description: {request.Description}";

        // Anthropic Messages API payload. Prefilling the assistant turn with "{" strongly
        // biases the model to return raw JSON.
        var payload = new
        {
            anthropic_version = "bedrock-2023-05-31",
            max_tokens = _settings.MaxTokens,
            temperature = _settings.Temperature,
            system = SystemPrompt,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[] { new { type = "text", text = userPrompt } },
                },
                new
                {
                    role = "assistant",
                    content = new object[] { new { type = "text", text = "{" } },
                },
            },
        };

        InvokeModelResponse response;
        try
        {
            using var body = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(payload));
            response = await _bedrock.InvokeModelAsync(
                new InvokeModelRequest
                {
                    ModelId = _settings.ModelId,
                    ContentType = "application/json",
                    Accept = "application/json",
                    Body = body,
                },
                cancellationToken);
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            _logger.LogError(ex, "Bedrock InvokeModel failed for model {ModelId}", _settings.ModelId);
            throw new CostEstimationException("The cost estimation service is currently unavailable.", ex);
        }

        var modelText = ExtractModelText(response);

        // Re-attach the prefilled "{" the assistant was primed with.
        var json = ExtractJsonObject("{" + modelText);
        if (json is null)
        {
            _logger.LogWarning("Bedrock response did not contain parseable JSON: {Raw}", modelText);
            throw new CostEstimationException("The cost estimate could not be interpreted. Please try again.");
        }

        try
        {
            var estimate = JsonSerializer.Deserialize<CostEstimateResponseDto>(json, ResponseJsonOptions)
                ?? throw new CostEstimationException("Empty cost estimate returned.");

            Normalize(estimate);
            return estimate;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize Bedrock cost estimate: {Json}", json);
            throw new CostEstimationException("The cost estimate could not be interpreted. Please try again.", ex);
        }
    }

    private static string ExtractModelText(InvokeModelResponse response)
    {
        using var reader = new StreamReader(response.Body, Encoding.UTF8);
        var raw = reader.ReadToEnd();

        using var doc = JsonDocument.Parse(raw);
        if (!doc.RootElement.TryGetProperty("content", out var content)
            || content.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var block in content.EnumerateArray())
        {
            if (block.TryGetProperty("type", out var type)
                && type.GetString() == "text"
                && block.TryGetProperty("text", out var text))
            {
                sb.Append(text.GetString());
            }
        }

        return sb.ToString();
    }

    /// <summary>Returns the first complete top-level JSON object found in the text, or null.</summary>
    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        if (start < 0) return null;

        var depth = 0;
        var inString = false;
        var escaped = false;

        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];

            if (inString)
            {
                if (escaped) escaped = false;
                else if (c == '\\') escaped = true;
                else if (c == '"') inString = false;
                continue;
            }

            switch (c)
            {
                case '"':
                    inString = true;
                    break;
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        return text.Substring(start, i - start + 1);
                    }
                    break;
            }
        }

        return null;
    }

    private static void Normalize(CostEstimateResponseDto estimate)
    {
        estimate.LowEstimate = Math.Max(0, estimate.LowEstimate);
        estimate.MostLikely = Math.Max(0, estimate.MostLikely);
        estimate.HighEstimate = Math.Max(0, estimate.HighEstimate);

        // Guarantee low <= most_likely <= high regardless of model ordering.
        var ordered = new[] { estimate.LowEstimate, estimate.MostLikely, estimate.HighEstimate };
        Array.Sort(ordered);
        estimate.LowEstimate = ordered[0];
        estimate.MostLikely = ordered[1];
        estimate.HighEstimate = ordered[2];

        estimate.Currency = string.IsNullOrWhiteSpace(estimate.Currency) ? "USD" : estimate.Currency.ToUpperInvariant();

        estimate.Confidence = estimate.Confidence?.ToLowerInvariant() switch
        {
            "high" => "high",
            "medium" => "medium",
            _ => "low",
        };

        estimate.Reasoning = estimate.Reasoning?.Trim() ?? string.Empty;
    }
}
