using System.ComponentModel.DataAnnotations;

namespace ZenBudget.Api.Models;

public class User
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "PHP"; // Philippine Peso

    [MaxLength(20)]
    public string Appearance { get; set; } = "system";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
