using Microsoft.AspNetCore.Mvc;
using ZenBudget.Api.Models.DTOs;
using ZenBudget.Api.Services;

namespace ZenBudget.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    /// <summary>
    /// Register a new user account. Seeds default budget categories.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "All fields are required." });

            if (request.Password.Length < 8)
                return BadRequest(new { error = "Password must be at least 8 characters." });

            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Authenticate and receive a JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
