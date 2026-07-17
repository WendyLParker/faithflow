using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Backend.Models;

public class Request
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Content { get; set; }

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedDate { get; set; }

    public ICollection<RequestGroup> AssignedGroups { get; set; } = new List<RequestGroup>();

    public int RequestTypeId { get; set; }
    public RequestType RequestType { get; set; } = null!;

    public RequestStatus RequestStatus { get; set; } = RequestStatus.New;

    public DateTime? FulfilledDate { get; set; }

    public string? RequesterEmail { get; set; }

    public string? VoiceNoteUrl { get; set; }
    public string? ImageUrl { get; set; }

    public int StreakDays { get; set; } = 0;

    public ICollection<ProgressNote> ProgressNotes { get; set; } = new List<ProgressNote>();
    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
}
