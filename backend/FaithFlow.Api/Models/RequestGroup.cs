namespace FaithFlow.Backend.Models;

/// <summary>Join table: which groups a request is assigned to.</summary>
public class RequestGroup
{
    public int RequestId { get; set; }
    public Request Request { get; set; } = null!;

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
}
