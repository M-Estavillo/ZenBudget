using System.ComponentModel.DataAnnotations;

namespace ZenBudget.Api.Models;

public class Transaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [Required, MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Negative for expenses, positive for income
    /// </summary>
    public decimal Amount { get; set; }

    public Guid CategoryId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? Location { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
