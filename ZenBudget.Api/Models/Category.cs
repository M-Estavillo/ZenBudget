using System.ComponentModel.DataAnnotations;

namespace ZenBudget.Api.Models;

public class Category
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string IconName { get; set; } = "ph:folder-thin";

    public decimal BudgetLimit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
