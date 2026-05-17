using Microsoft.EntityFrameworkCore;
using ZenBudget.Api.Data;

namespace ZenBudget.Api.Services;

public static class CategoryColorHelper 
{
    public static readonly string[] ChartColors = [
        "#2E5BFF", // Cobalt Blue
        "#FF6B6B", // Coral Red
        "#4ECDC4", // Turquoise
        "#FFE66D", // Lemon Yellow
        "#FF9F1C", // Orange
        "#9D4EDD", // Purple
        "#38B000", // Apple Green
        "#EF476F", // Pink
        "#118AB2", // Ocean Blue
        "#06D6A0", // Mint
        "#FFD166", // Mustard
        "#F72585", // Neon Pink
        "#4361EE", // Indigo
        "#7209B7", // Deep Violet
        "#4CC9F0"  // Sky Blue
    ];

    public static async Task<Dictionary<Guid, string>> GetColorsAsync(ZenBudgetDbContext db, Guid userId)
    {
        var categoryIds = await db.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => c.Id)
            .ToListAsync();

        var dict = new Dictionary<Guid, string>();
        for (int i = 0; i < categoryIds.Count; i++)
        {
            dict[categoryIds[i]] = ChartColors[i % ChartColors.Length];
        }
        return dict;
    }
}
