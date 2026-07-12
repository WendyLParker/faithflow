using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Backend.Models;

public class Group
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<UserGroup> Members { get; set; } = new List<UserGroup>();
}
