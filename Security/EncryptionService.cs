using System.Security.Cryptography;
using System.Text;

namespace SecurePasswordManager.Security;

public class EncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var key = configuration["EncryptionKey"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("EncryptionKey is not configured.");
        }

        _key = Convert.FromBase64String(key);

        if (_key.Length != 32)
        {
            throw new InvalidOperationException("EncryptionKey must be 32 bytes.");
        }
    }

    public string Encrypt(string plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key, 16);

        aes.Encrypt(
            nonce,
            plaintextBytes,
            ciphertext,
            tag
        );

        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];

        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(
            ciphertext,
            0,
            result,
            nonce.Length + tag.Length,
            ciphertext.Length
        );

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encryptedText)
    {
        var data = Convert.FromBase64String(encryptedText);

        var nonce = data[..12];
        var tag = data[12..28];
        var ciphertext = data[28..];

        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, 16);

        aes.Decrypt(
            nonce,
            ciphertext,
            tag,
            plaintext
        );

        return Encoding.UTF8.GetString(plaintext);
    }
    
public byte[] EncryptBytes(byte[] plaintext)
{
    var nonce = RandomNumberGenerator.GetBytes(12);
    var ciphertext = new byte[plaintext.Length];
    var tag = new byte[16];

    using var aes = new AesGcm(_key, 16);

    aes.Encrypt(nonce, plaintext, ciphertext, tag);

    var result = new byte[nonce.Length + tag.Length + ciphertext.Length];

    Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
    Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
    Buffer.BlockCopy(
        ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length
    );

    return result;
}

public byte[] DecryptBytes(byte[] encryptedData)
{
    if (encryptedData == null || encryptedData.Length < 28)
    {
        throw new CryptographicException("Invalid encrypted file data.");
    }

    var nonce = encryptedData[..12];
    var tag = encryptedData[12..28];
    var ciphertext = encryptedData[28..];
    var plaintext = new byte[ciphertext.Length];

    using var aes = new AesGcm(_key, 16);

    aes.Decrypt(nonce, ciphertext, tag, plaintext);

    return plaintext;
}

}