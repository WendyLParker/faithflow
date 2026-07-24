using FaithFlow.Backend.Common;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithFlow.Backend.Controllers;

[Route("api/requests")]
[ApiController]
[Authorize]
public class CostEstimationController : ControllerBase
{
    private readonly ICostEstimationService _estimation;
    private readonly ILogger<CostEstimationController> _logger;

    public CostEstimationController(
        ICostEstimationService estimation,
        ILogger<CostEstimationController> logger)
    {
        _estimation = estimation;
        _logger = logger;
    }

    /// <summary>
    /// Returns an optional AI-generated cost estimate for a request. This does not create
    /// or modify a request; it is purely advisory.
    /// </summary>
    [HttpPost("estimate-cost")]
    [ProducesResponseType(typeof(CostEstimateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<CostEstimateResponseDto>> EstimateCost(
        [FromBody] CostEstimateRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null
            || (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Description)))
        {
            return BadRequest(new { error = "A title or description is required to estimate cost." });
        }

        try
        {
            var estimate = await _estimation.EstimateAsync(request, cancellationToken);
            return Ok(estimate);
        }
        catch (CostEstimationException ex)
        {
            _logger.LogWarning(ex, "Cost estimation failed");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = ex.Message });
        }
    }
}
