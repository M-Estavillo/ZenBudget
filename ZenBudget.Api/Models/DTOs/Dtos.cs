namespace ZenBudget.Api.Models.DTOs;

// ── Auth DTOs ──
public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, UserDto User);

// ── User DTOs ──
public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? AvatarUrl,
    string Currency,
    string Appearance
);

public record UpdateProfileRequest(string? FullName, string? Currency, string? Appearance);

// ── Transaction DTOs ──
public record TransactionDto(
    Guid Id,
    string Description,
    decimal Amount,
    Guid CategoryId,
    string? CategoryName,
    string? CategoryIcon,
    DateTime Timestamp,
    string? Location
);

public record CreateTransactionRequest(
    string Description,
    decimal Amount,
    Guid CategoryId,
    DateTime? Timestamp,
    string? Location
);

public record UpdateTransactionRequest(
    string? Description,
    decimal? Amount,
    Guid? CategoryId,
    DateTime? Timestamp,
    string? Location
);

// ── Category DTOs ──
public record CategoryDto(
    Guid Id,
    string Name,
    string IconName,
    decimal BudgetLimit,
    decimal CurrentSpending,
    int Percentage,
    string Color
);

public record CreateCategoryRequest(string Name, string? IconName, decimal BudgetLimit);
public record UpdateCategoryRequest(string? Name, string? IconName, decimal? BudgetLimit);

// ── Analytics DTOs ──
public record WeeklySummaryDto(string DayLabel, decimal Amount, bool IsHighlighted);
public record CategorySpendingDto(string Name, decimal Amount, int Percentage, string Color);
public record MonthlyTrendDto(string Month, decimal Amount, bool IsHighlighted);
public record DashboardDto(
    decimal CurrentBalance,
    List<WeeklySummaryDto> WeeklySpending,
    List<TransactionDto> RecentTransactions
);
