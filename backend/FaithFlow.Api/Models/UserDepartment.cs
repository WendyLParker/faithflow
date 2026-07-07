using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Backend.Models;

/// <summary>Maps a Cognito user to a department. Stored by admin; used for notification routing.</summary>
public class UserDepartment
{
    public int Id { get; set; }

    /// <summary>Cognito sub (user ID).</summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Display name entered by admin.</summary>
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Email for sending notifications.</summary>
    [MaxLength(320)]
    public string UserEmail { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public bool EmailNotificationsEnabled { get; set; } = true;
}
