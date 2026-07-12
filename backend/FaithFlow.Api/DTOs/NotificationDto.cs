namespace FaithFlow.Backend.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public int RequestId { get; set; }
    public string RequestTitle { get; set; } = string.Empty;
    public string? RequestContent { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public string RequestStatus { get; set; } = string.Empty;
}

public class UnreadCountDto
{
    public int Count { get; set; }
}
