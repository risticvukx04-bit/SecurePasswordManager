
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurePasswordManager.Data;
using SecurePasswordManager.Models;
using SecurePasswordManager.Security;

namespace SecurePasswordManager.Controllers;

[ApiController]
[Route("files")]
public class EncryptedFilesController : ControllerBase
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private readonly ApplicationDbContext _context;
    private readonly SessionService _sessionService;
    private readonly EncryptionService _encryptionService;
    private readonly IWebHostEnvironment _environment;

    public EncryptedFilesController(
        ApplicationDbContext context,
        SessionService sessionService,
        EncryptionService encryptionService,
        IWebHostEnvironment environment)
    {
        _context = context;
        _sessionService = sessionService;
        _encryptionService = encryptionService;
        _environment = environment;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSize + 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var userId = await _sessionService.GetUserIdFromSession(Request);

        if (userId == null)
            return Unauthorized("You must be logged in.");

        if (file == null || file.Length == 0)
            return BadRequest("Choose a file to upload.");

        if (file.Length > MaxFileSize)
            return BadRequest("Maximum file size is 10 MB.");

        var originalFileName = Path.GetFileName(file.FileName);

        if (string.IsNullOrWhiteSpace(originalFileName))
            return BadRequest("Invalid file name.");

        var storedFileName = $"{Guid.NewGuid():N}.enc";
        var storageDirectory = Path.Combine(
            _environment.ContentRootPath, "App_Data", "EncryptedFiles");

        Directory.CreateDirectory(storageDirectory);

        var storagePath = Path.Combine(storageDirectory, storedFileName);

        try
        {
            await using var input = new MemoryStream();
            await file.CopyToAsync(input);

            var encryptedBytes = _encryptionService.EncryptBytes(input.ToArray());
            await System.IO.File.WriteAllBytesAsync(storagePath, encryptedBytes);

            var record = new EncryptedFile
            {
                UserId = userId.Value,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                UploadedAt = DateTime.UtcNow
            };

            _context.EncryptedFiles.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                record.Id,
                record.OriginalFileName,
                record.UploadedAt,
                message = "File encrypted and uploaded successfully."
            });
        }
        catch
        {
            if (System.IO.File.Exists(storagePath))
                System.IO.File.Delete(storagePath);

            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> ListFiles()
    {
        var userId = await _sessionService.GetUserIdFromSession(Request);

        if (userId == null)
            return Unauthorized("You must be logged in.");

        var files = await _context.EncryptedFiles
            .Where(f => f.UserId == userId.Value)
            .OrderByDescending(f => f.UploadedAt)
            .Select(f => new
            {
                f.Id,
                f.OriginalFileName,
                f.UploadedAt
            })
            .ToListAsync();

        return Ok(files);
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var userId = await _sessionService.GetUserIdFromSession(Request);

        if (userId == null)
            return Unauthorized("You must be logged in.");

        var record = await _context.EncryptedFiles
            .FirstOrDefaultAsync(f =>
                f.Id == id && f.UserId == userId.Value);

        if (record == null)
            return NotFound("File not found.");

        var storagePath = Path.Combine(
            _environment.ContentRootPath,
            "App_Data",
            "EncryptedFiles",
            record.StoredFileName);

        if (!System.IO.File.Exists(storagePath))
            return NotFound("Stored file not found.");

        try
        {
            var encryptedBytes = await System.IO.File.ReadAllBytesAsync(storagePath);
            var plaintext = _encryptionService.DecryptBytes(encryptedBytes);

            return File(
                plaintext,
                "application/octet-stream",
                record.OriginalFileName);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return StatusCode(500, "File integrity verification failed.");
        }
    }
}
