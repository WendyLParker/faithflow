namespace FaithFlow.Backend.DTOs;

public class RequestCommentCreateDto
{
    public string Content { get; set; } = string.Empty;
}

public class RequestCommentResponseDto
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsOwnComment { get; set; }
}
