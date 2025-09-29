namespace TestLoginAppFrontend.Models;

public class LoginViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string IPs { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
