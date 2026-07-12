namespace FaithFlow.Backend.DTOs;

public class UserRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class MyRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfileColor { get; set; }
}

public class UpdateProfileDto
{
    public string? DisplayName { get; set; }
    public string? ProfileColor { get; set; }
}

public class SetUserRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
