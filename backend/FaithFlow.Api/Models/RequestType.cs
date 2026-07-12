namespace FaithFlow.Backend.Models;

public class RequestType
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
