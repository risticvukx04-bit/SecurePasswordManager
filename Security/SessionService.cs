using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SecurePasswordManager.Data;
using SecurePasswordManager.Models;

namespace SecurePasswordManager.Security;

public class SessionService
{
    private readonly ApplicationDbContext _context;

    public SessionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateSession(HttpResponse response, int userId)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes);

        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token))
        );

        var session = new Session
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        response.Cookies.Append(
            "session",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            }
        );
    }

    public async Task<int?> GetUserIdFromSession(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("session", out var token))
        {
            return null;
        }

        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token))
        );

        var session = await _context.Sessions
            .FirstOrDefaultAsync(s =>
                s.TokenHash == tokenHash &&
                s.ExpiresAt > DateTime.UtcNow);

        if (session == null)
        {
            return null;
        }

        return session.UserId;
    }

    public async Task ClearSession(HttpRequest request, HttpResponse response)
    {
        if (request.Cookies.TryGetValue("session", out var token))
        {
            var tokenHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(token))
            );

            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.TokenHash == tokenHash);

            if (session != null)
            {
                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
            }
        }

        response.Cookies.Delete("session");
    }
}