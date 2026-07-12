namespace FaithFlow.Backend.DTOs;

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class GroupMemberDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public bool EmailNotificationsEnabled { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool CanManage { get; set; }
}

public class CreateGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AddGroupMemberDto
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

public class UpdateManagerDto
{
    public bool CanManage { get; set; }
}

public class GroupManagerAssignmentDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool CanManage { get; set; }
}

public class SetGroupManagersDto
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<int> GroupIds { get; set; } = new();
}
