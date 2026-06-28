namespace FaithFlow.Backend.DTOs;

public class ProgressNoteCreateDto
{
    public int PrayerId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? MoodRating { get; set; }
}