namespace FaithFlow.Backend.Common;

/// <summary>Raised when an AI cost estimate cannot be produced.</summary>
public class CostEstimationException : Exception
{
    public CostEstimationException(string message) : base(message)
    {
    }

    public CostEstimationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
