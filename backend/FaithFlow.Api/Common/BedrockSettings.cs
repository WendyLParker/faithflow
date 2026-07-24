namespace FaithFlow.Backend.Common;

/// <summary>
/// Configuration for the Amazon Bedrock cost estimation feature.
/// Bind from the "Bedrock" section of configuration.
/// </summary>
public class BedrockSettings
{
    /// <summary>AWS region where Bedrock model access is enabled (e.g. "us-east-1").</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Bedrock model identifier. Defaults to the Claude 3.5 Sonnet v2 cross-region
    /// inference profile. Some accounts must use the plain model id
    /// ("anthropic.claude-3-5-sonnet-20241022-v2:0") instead of the "us." profile.
    /// </summary>
    public string ModelId { get; set; } = "us.anthropic.claude-3-5-sonnet-20241022-v2:0";

    /// <summary>Maximum tokens for the model response.</summary>
    public int MaxTokens { get; set; } = 512;

    /// <summary>Sampling temperature. Low values keep estimates deterministic.</summary>
    public float Temperature { get; set; } = 0.2f;
}
