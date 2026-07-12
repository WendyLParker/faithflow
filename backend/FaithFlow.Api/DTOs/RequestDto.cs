namespace FaithFlow.Backend.DTOs;

public class RequestResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime RequestDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int RequestTypeId { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public List<string> GroupNames { get; set; } = new();
    public string RequestStatus { get; set; } = "New";
    public string? VoiceNoteUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int StreakDays { get; set; }
}

public class RequestCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int RequestTypeId { get; set; }
    public List<int> GroupIds { get; set; } = new();
}

public class RequestUpdateDto
{
    public string? Content { get; set; }
    public bool? IsCompleted { get; set; }
}
