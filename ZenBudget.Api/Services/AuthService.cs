using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ZenBudget.Api.Data;
using ZenBudget.Api.Models;
using ZenBudget.Api.Models.DTOs;

namespace ZenBudget.Api.Services;

public class AuthService
{
    private readonly ZenBudgetDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(ZenBudgetDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);

        // Seed default categories for Filipino users
        var defaultCategories = new[]
        {
            new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Food & Dining", IconName = "ph:fork-knife-thin", BudgetLimit = 8000 },
            new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Transport", IconName = "ph:car-thin", BudgetLimit = 3000 },
            new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Entertainment", IconName = "ph:music-notes-thin", BudgetLimit = 2000 },
            new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Rent & Housing", IconName = "ph:house-thin", BudgetLimit = 15000 },
            new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Health", IconName = "ph:heartbeat-thin", BudgetLimit = 2000 },
            new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Groceries", IconName = "ph:shopping-cart-thin", BudgetLimit = 5000 },
        };

        _db.Categories.AddRange(defaultCategories);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return new AuthResponse(token, MapToDto(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = GenerateJwtToken(user);
        return new AuthResponse(token, MapToDto(user));
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToDto(User user) => new(
        user.Id, user.FullName, user.Email,
        user.AvatarUrl, user.Currency, user.Appearance
    );
}
