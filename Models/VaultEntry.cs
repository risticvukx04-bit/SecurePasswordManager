namespace SecurePasswordManager.Models;

public class VaultEntry
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Website { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string EncryptedPassword { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}