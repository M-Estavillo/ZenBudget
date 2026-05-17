using Microsoft.EntityFrameworkCore;
using ZenBudget.Api.Data;
using ZenBudget.Api.Models;
using ZenBudget.Api.Models.DTOs;

namespace ZenBudget.Api.Services;

public class TransactionService
{
    private readonly ZenBudgetDbContext _db;

    public TransactionService(ZenBudgetDbContext db) => _db = db;

    public async Task<List<TransactionDto>> GetAllAsync(Guid userId, string? search = null, Guid? categoryId = null)
    {
        var query = _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Description.ToLower().Contains(search.ToLower()));

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        return await query
            .OrderByDescending(t => t.Timestamp)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid userId, Guid id)
    {
        var t = await _db.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        return t == null ? null : MapToDto(t);
    }

    public async Task<TransactionDto> CreateAsync(Guid userId, CreateTransactionRequest request)
    {
        // Verify category belongs to user
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == userId);
        if (category == null)
            throw new InvalidOperationException("Category not found.");

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Description = request.Description,
            Amount = request.Amount,
            CategoryId = request.CategoryId,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Location = request.Location
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        // Re-load with category for response
        transaction.Category = category;
        return MapToDto(transaction);
    }

    public async Task<TransactionDto?> UpdateAsync(Guid userId, Guid id, UpdateTransactionRequest request)
    {
        var transaction = await _db.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null) return null;

        if (request.Description != null) transaction.Description = request.Description;
        if (request.Amount.HasValue) transaction.Amount = request.Amount.Value;
        if (request.Timestamp.HasValue) transaction.Timestamp = request.Timestamp.Value;
        if (request.Location != null) transaction.Location = request.Location;

        if (request.CategoryId.HasValue)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value && c.UserId == userId);
            if (category == null) throw new InvalidOperationException("Category not found.");
            transaction.CategoryId = request.CategoryId.Value;
            transaction.Category = category;
        }

        await _db.SaveChangesAsync();
        return MapToDto(transaction);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (transaction == null) return false;

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<WeeklySummaryDto>> GetWeeklySummaryAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek); // Sunday
        var dayLabels = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        var weeklyData = await _db.Transactions
            .Where(t => t.UserId == userId && t.Timestamp >= startOfWeek && t.Amount < 0)
            .GroupBy(t => t.Timestamp.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(t => Math.Abs(t.Amount)) })
            .ToListAsync();

        var result = new List<WeeklySummaryDto>();
        var maxAmount = weeklyData.Any() ? weeklyData.Max(d => d.Total) : 0;

        for (int i = 0; i < 7; i++)
        {
            var date = startOfWeek.AddDays(i);
            var dayData = weeklyData.FirstOrDefault(d => d.Date == date);
            var amount = dayData?.Total ?? 0;
            result.Add(new WeeklySummaryDto(dayLabels[i], amount, date == today));
        }

        return result;
    }

    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        return await _db.Transactions
            .Where(t => t.UserId == userId)
            .SumAsync(t => t.Amount);
    }

    public async Task<List<TransactionDto>> GetRecentAsync(Guid userId, int count = 5)
    {
        return await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    private static TransactionDto MapToDto(Transaction t) => new(
        t.Id, t.Description, t.Amount, t.CategoryId,
        t.Category?.Name, t.Category?.IconName,
        t.Timestamp, t.Location
    );
}
