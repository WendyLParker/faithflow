namespace FaithFlow.Backend.DTOs;

public class PrayerResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime PrayerDate { get; set; }
    public bool IsAnswered { get; set; }
    public DateTime? AnsweredDate { get; set; }
    public List<string> Categories { get; set; } = new();
    public string? VoiceNoteUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int StreakDays { get; set; }
}