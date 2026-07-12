namespace FaithFlow.Backend.Models;

public class ProgressNote
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    public int RequestId { get; set; }
    public string Content { get; set; } = string.Empty;

    public DateTime EntryDate { get; set; } = DateTime.UtcNow;

    public int? MoodRating { get; set; }
    public Request Request { get; set; } = null!;
}
