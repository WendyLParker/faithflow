namespace FaithFlow.Backend.DTOs;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> RequestTypeNames { get; set; } = new();
}

public class DepartmentMemberDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public bool EmailNotificationsEnabled { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
}

public class AddDepartmentMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public bool EmailNotificationsEnabled { get; set; } = true;
}

public class UpdateEmailPreferenceDto
{
    public bool EmailNotificationsEnabled { get; set; }
}
