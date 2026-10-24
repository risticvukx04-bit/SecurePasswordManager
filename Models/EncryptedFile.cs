
namespace SecurePasswordManager.Models;

public class EncryptedFile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
