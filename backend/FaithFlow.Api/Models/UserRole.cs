using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Backend.Models;

/// <summary>Tracks the application role (Admin/Member) for a Cognito user.</summary>
public class UserRole
{
    public int Id { get; set; }

    /// <summary>Cognito sub (user ID).</summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(320)]
    public string UserEmail { get; set; } = string.Empty;

    public AppRole Role { get; set; } = AppRole.Member;

    /// <summary>User-chosen accent color (CSS hex string, e.g. "#34c759").</summary>
    [MaxLength(20)]
    public string? ProfileColor { get; set; }
}
