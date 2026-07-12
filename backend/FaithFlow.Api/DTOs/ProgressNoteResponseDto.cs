namespace FaithFlow.Backend.DTOs;

public class ProgressNoteResponseDto
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public int? MoodRating { get; set; }
}
