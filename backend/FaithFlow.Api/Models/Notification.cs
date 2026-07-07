namespace FaithFlow.Backend.Models;

public class Notification
{
    public int Id { get; set; }

    /// <summary>Cognito sub of the user who should receive this notification.</summary>
    public string RecipientUserId { get; set; } = string.Empty;

    public int PrayerId { get; set; }
    public Prayer Prayer { get; set; } = null!;

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
