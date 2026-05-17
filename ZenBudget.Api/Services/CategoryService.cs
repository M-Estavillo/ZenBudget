using Microsoft.EntityFrameworkCore;
using ZenBudget.Api.Data;
using ZenBudget.Api.Models;
using ZenBudget.Api.Models.DTOs;

namespace ZenBudget.Api.Services;

public class CategoryService
{
    private readonly ZenBudgetDbContext _db;

    public CategoryService(ZenBudgetDbContext db) => _db = db;

    public async Task<List<CategoryDto>> GetAllAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var categories = await _db.Categories
            .Where(c => c.UserId == userId)
            .Select(c => new
            {
                Category = c,
                CurrentSpending = c.Transactions
                    .Where(t => t.Timestamp >= startOfMonth && t.Amount < 0)
                    .Sum(t => Math.Abs(t.Amount))
            })
            .ToListAsync();

        var colors = await CategoryColorHelper.GetColorsAsync(_db, userId);

        return categories.Select(c =>
        {
            var pct = c.Category.BudgetLimit > 0
                ? (int)Math.Round(c.CurrentSpending / c.Category.BudgetLimit * 100)
                : 0;

            return new CategoryDto(
                c.Category.Id, c.Category.Name, c.Category.IconName,
                c.Category.BudgetLimit, c.CurrentSpending, pct,
                colors.GetValueOrDefault(c.Category.Id, "#71717A")
            );
        }).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid userId, Guid id)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = await _db.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(c => new
            {
                Category = c,
                CurrentSpending = c.Transactions
                    .Where(t => t.Timestamp >= startOfMonth && t.Amount < 0)
                    .Sum(t => Math.Abs(t.Amount))
            })
            .FirstOrDefaultAsync();

        if (result == null) return null;

        var pct = result.Category.BudgetLimit > 0
            ? (int)Math.Round(result.CurrentSpending / result.Category.BudgetLimit * 100)
            : 0;

        var colors = await CategoryColorHelper.GetColorsAsync(_db, userId);

        return new CategoryDto(
            result.Category.Id, result.Category.Name, result.Category.IconName,
            result.Category.BudgetLimit, result.CurrentSpending, pct,
            colors.GetValueOrDefault(result.Category.Id, "#71717A")
        );
    }

    public async Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            IconName = request.IconName ?? "ph:folder-thin",
            BudgetLimit = request.BudgetLimit
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        var colors = await CategoryColorHelper.GetColorsAsync(_db, userId);

        return new CategoryDto(category.Id, category.Name, category.IconName,
            category.BudgetLimit, 0, 0, colors.GetValueOrDefault(category.Id, "#71717A"));
    }

    public async Task<CategoryDto?> UpdateAsync(Guid userId, Guid id, UpdateCategoryRequest request)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (category == null) return null;

        if (request.Name != null) category.Name = request.Name;
        if (request.IconName != null) category.IconName = request.IconName;
        if (request.BudgetLimit.HasValue) category.BudgetLimit = request.BudgetLimit.Value;

        await _db.SaveChangesAsync();

        // Re-fetch with spending
        return await GetByIdAsync(userId, id);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var category = await _db.Categories
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null) return false;

        if (category.Transactions.Any())
            throw new InvalidOperationException("Cannot delete a category that has transactions. Delete or reassign the transactions first.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }
}
