using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurePasswordManager.Data;
using SecurePasswordManager.DTOs;
using SecurePasswordManager.Models;
using SecurePasswordManager.Security;

namespace SecurePasswordManager.Controllers;

[ApiController]
[Route("vault")]
public class VaultController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly SessionService _sessionService;
    private readonly EncryptionService _encryptionService;

    public VaultController(
        ApplicationDbContext context,
      SessionService sessionService,
EncryptionService encryptionService)
    {
        _context = context;
        _sessionService = sessionService;
        _encryptionService = encryptionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVault()
    {
        var userId = await _sessionService.GetUserIdFromSession(Request);

        if (userId == null)
        {
            return Unauthorized("You must be logged in.");
        }

        var entries = await _context.VaultEntries
            .Where(v => v.UserId == userId.Value)
            .ToListAsync();

        return Ok(entries.Select(e => new
{
    e.Id,
    e.Website,
    e.Username,
    Password = _encryptionService.Decrypt(e.EncryptedPassword),
    e.CreatedAt
}));
    }

    [HttpPost]
   public async Task<IActionResult> AddEntry(SecurePasswordManager.DTOs.VaultEntryRequest request)
    {
        var userId = await _sessionService.GetUserIdFromSession(Request);

        if (userId == null)
        {
            return Unauthorized("You must be logged in.");
        }

        if (string.IsNullOrWhiteSpace(request.Website) ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("All fields are required.");
        }

        var entry = new VaultEntry
        {
            UserId = userId.Value,
            Website = request.Website,
            Username = request.Username,
            EncryptedPassword = _encryptionService.Encrypt(request.Password)
        };

        _context.VaultEntries.Add(entry);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Vault entry added successfully."
        });
    }
    [HttpPut("{id}")]
public async Task<IActionResult> UpdateEntry(int id, VaultEntryRequest request)
{
    var userId = await _sessionService.GetUserIdFromSession(Request);

    if (userId == null)
    {
        return Unauthorized("You must be logged in.");
    }

    if (string.IsNullOrWhiteSpace(request.Website) ||
        string.IsNullOrWhiteSpace(request.Username) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return BadRequest("All fields are required.");
    }

    var entry = await _context.VaultEntries
        .FirstOrDefaultAsync(v =>
            v.Id == id &&
            v.UserId == userId.Value);

    if (entry == null)
    {
        return NotFound("Vault entry not found.");
    }

    entry.Website = request.Website;
    entry.Username = request.Username;
    entry.EncryptedPassword = _encryptionService.Encrypt(request.Password);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Vault entry updated successfully."
    });
}
[HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteEntry(int id)
{
    var userId = await _sessionService.GetUserIdFromSession(Request);

    if (userId == null)
    {
        return Unauthorized("You must be logged in.");
    }

    var entry = await _context.VaultEntries
        .FirstOrDefaultAsync(v =>
            v.Id == id &&
            v.UserId == userId.Value);

    if (entry == null)
    {
        return NotFound("Vault entry not found.");
    }

    _context.VaultEntries.Remove(entry);
    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Vault entry deleted successfully."
    });
}
}