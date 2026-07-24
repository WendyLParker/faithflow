using System.Text.Json.Serialization;

namespace FaithFlow.Backend.DTOs;

/// <summary>Input for an AI cost estimate on a request.</summary>
public class CostEstimateRequestDto
{
    /// <summary>The request type name (e.g. "Supply", "Travel", "Services", "IT").</summary>
    public string RequestType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Structured cost estimate returned to the client. Serialized as snake_case to
/// match the contract expected by the frontend and produced by the model.
/// </summary>
public class CostEstimateResponseDto
{
    [JsonPropertyName("low_estimate")]
    public decimal LowEstimate { get; set; }

    [JsonPropertyName("most_likely")]
    public decimal MostLikely { get; set; }

    [JsonPropertyName("high_estimate")]
    public decimal HighEstimate { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("confidence")]
    public string Confidence { get; set; } = "low";

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;
}
