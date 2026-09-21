
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurePasswordManager.Data;
using SecurePasswordManager.DTOs;
using SecurePasswordManager.Models;
using SecurePasswordManager.Services;
using SecurePasswordManager.Security;

namespace SecurePasswordManager.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly SessionService _sessionService;

    public AuthController(
        ApplicationDbContext context,
        PasswordService passwordService,
        SessionService sessionService)
    {
        _context = context;
        _passwordService = passwordService;
        _sessionService = sessionService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (existingUser != null)
        {
            return Conflict("Username already exists.");
        }

        var user = new User
        {
            Username = request.Username
        };

        user.PasswordHash = _passwordService.HashPassword(
            user,
            request.Password
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User registered successfully."
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        var passwordValid = _passwordService.VerifyPassword(
            user,
            request.Password,
            user.PasswordHash
        );

        if (!passwordValid)
        {
            return Unauthorized("Invalid username or password.");
        }
Console.WriteLine("BEFORE SESSION");
       await _sessionService.CreateSession(Response, user.Id);

        return Ok(new
        {
            message = "Login successful."
        });
    }
    [HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    await _sessionService.ClearSession(Request, Response);

    return Ok(new
    {
        message = "Logout successful."
    });
}
}

