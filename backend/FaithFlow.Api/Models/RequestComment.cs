using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Backend.Models;

public class RequestComment
{
    public int Id { get; set; }

    public int RequestId { get; set; }
    public Request Request { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
