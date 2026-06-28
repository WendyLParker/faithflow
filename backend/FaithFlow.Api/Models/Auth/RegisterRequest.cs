namespace FaithFlow.Backend.Models.Auth;
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // Add more attributes later (name, phone, etc.)
}