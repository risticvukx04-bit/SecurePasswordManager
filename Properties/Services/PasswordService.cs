using Microsoft.AspNetCore.Identity;
using SecurePasswordManager.Models;

namespace SecurePasswordManager.Services;

public class PasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(
            user,
            passwordHash,
            password
        );

        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}