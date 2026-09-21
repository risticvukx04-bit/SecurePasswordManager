namespace SecurePasswordManager.DTOs;

public class VaultEntryRequest
{
    public string Website { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}