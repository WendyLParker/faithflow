namespace FaithFlow.Backend.Models.Auth;

public class ConfirmRequest
{
    public string Username { get; set; } = string.Empty;
    public string ConfirmationCode { get; set; } = string.Empty;
}