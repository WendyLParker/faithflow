using FaithFlow.Backend.Models;

namespace FaithFlow.Backend.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    // Embedded request info
    public int PrayerId { get; set; }
    public string PrayerTitle { get; set; } = string.Empty;
    public string? PrayerContent { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public string RequestStatus { get; set; } = string.Empty;
}

public class UnreadCountDto
{
    public int Count { get; set; }
}
