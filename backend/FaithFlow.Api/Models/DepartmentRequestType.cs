namespace FaithFlow.Backend.Models;

/// <summary>Join table: which request types are handled by which department.</summary>
public class DepartmentRequestType
{
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int RequestTypeId { get; set; }
    public RequestType RequestType { get; set; } = null!;
}
