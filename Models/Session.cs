namespace SecurePasswordManager.Models;

public class Session
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public User? User { get; set; }
}