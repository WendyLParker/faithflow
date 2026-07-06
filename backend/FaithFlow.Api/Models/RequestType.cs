using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Backend.Models;

public class RequestType
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Prayer> Prayers { get; set; } = new List<Prayer>();
}
