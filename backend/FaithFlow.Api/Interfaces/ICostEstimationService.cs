using FaithFlow.Backend.DTOs;

namespace FaithFlow.Backend.Interfaces;

public interface ICostEstimationService
{
    /// <summary>
    /// Produces a structured cost estimate for a request using Amazon Bedrock (Claude).
    /// </summary>
    /// <exception cref="FaithFlow.Backend.Common.CostEstimationException">
    /// Thrown when the model call fails or returns an unusable response.
    /// </exception>
    Task<CostEstimateResponseDto> EstimateAsync(CostEstimateRequestDto request, CancellationToken cancellationToken = default);
}
