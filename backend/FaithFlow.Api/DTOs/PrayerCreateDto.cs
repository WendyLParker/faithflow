namespace FaithFlow.Backend.DTOs;

public class PrayerCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public List<string> Categories { get; set; } = new();
    public int RequestTypeId { get; set; }
}