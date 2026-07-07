using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Backend.Models;

public class Department
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<DepartmentRequestType> DepartmentRequestTypes { get; set; } = new List<DepartmentRequestType>();
    public ICollection<UserDepartment> Members { get; set; } = new List<UserDepartment>();
}
