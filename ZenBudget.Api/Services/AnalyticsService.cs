using Microsoft.EntityFrameworkCore;
using ZenBudget.Api.Data;
using ZenBudget.Api.Models.DTOs;

namespace ZenBudget.Api.Services;

public class AnalyticsService
{
    private readonly ZenBudgetDbContext _db;

    // Vibrant colors for the pie chart
    private static readonly string[] ChartColors = ["#2E5BFF", "#FF6B6B", "#4ECDC4", "#FFE66D", "#FF9F1C", "#9D4EDD", "#38B000"];

    public AnalyticsService(ZenBudgetDbContext db) => _db = db;

    public async Task<List<CategorySpendingDto>> GetSpendingByCategoryAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var spending = await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Timestamp >= startOfMonth && t.Amount < 0)
            .GroupBy(t => new { t.CategoryId, t.Category.Name })
            .Select(g => new { CategoryId = g.Key.CategoryId, CategoryName = g.Key.Name, Total = g.Sum(t => Math.Abs(t.Amount)) })
            .OrderByDescending(g => g.Total)
            .ToListAsync();

        var grandTotal = spending.Sum(s => s.Total);
        if (grandTotal == 0) return new List<CategorySpendingDto>();

        var colors = await CategoryColorHelper.GetColorsAsync(_db, userId);

        return spending.Select(s => new CategorySpendingDto(
            s.CategoryName,
            s.Total,
            (int)Math.Round(s.Total / grandTotal * 100),
            colors.GetValueOrDefault(s.CategoryId, "#71717A")
        )).ToList();
    }

    public async Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(Guid userId, int months = 12)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthlyData = await _db.Transactions
            .Where(t => t.UserId == userId && t.Timestamp >= startDate && t.Amount < 0)
            .GroupBy(t => new { t.Timestamp.Year, t.Timestamp.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(t => Math.Abs(t.Amount))
            })
            .ToListAsync();

        var result = new List<MonthlyTrendDto>();
        for (int i = 0; i < months; i++)
        {
            var date = startDate.AddMonths(i);
            var data = monthlyData.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);
            var amount = data?.Total ?? 0;
            var monthLabel = date.ToString("MMM");
            var isCurrent = date.Year == now.Year && date.Month == now.Month;
            result.Add(new MonthlyTrendDto(monthLabel, amount, isCurrent));
        }

        return result;
    }

    public async Task<decimal> GetTotalSpendingThisMonthAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return await _db.Transactions
            .Where(t => t.UserId == userId && t.Timestamp >= startOfMonth && t.Amount < 0)
            .SumAsync(t => Math.Abs(t.Amount));
    }
}
