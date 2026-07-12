namespace FaithFlow.Backend.Models;

public enum NotificationType
{
    /// <summary>Sent to every member of the handling group when a new request is submitted.</summary>
    NewRequest = 0,

    /// <summary>Sent to the original requester when a group member acknowledges their request.</summary>
    RequestAcknowledged = 1,
}
