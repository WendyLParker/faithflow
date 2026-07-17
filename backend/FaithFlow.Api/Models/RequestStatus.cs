namespace FaithFlow.Backend.Models;

public enum RequestStatus
{
    New = 0,
    Acknowledged = 1,
    /// <summary>Assignee has completed their part; waiting for requestor to close.</summary>
    Fulfilled = 2,
}
