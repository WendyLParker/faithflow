namespace FaithFlow.Backend.Models;

public class Notification
{
    public int Id { get; set; }

    public string RecipientUserId { get; set; } = string.Empty;

    public int RequestId { get; set; }
    public Request Request { get; set; } = null!;

    public NotificationType Type { get; set; }

    public int? CommentId { get; set; }
    public RequestComment? Comment { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
