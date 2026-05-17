#pragma warning disable IL2026

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AndroidBudgetingApp.Services;

/// <summary>
/// Singleton HTTP client wrapper for communicating with the ZenBudget API.
/// </summary>
public class ApiService
{
    private static ApiService? _instance;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Change this to your machine's local IP when testing on a physical device.
    // For Android emulator, 10.0.2.2 maps to host machine's localhost.
    private const string BaseUrl = "http://10.0.2.2:5000/api";

    private ApiService()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public static ApiService Instance => _instance ??= new ApiService();

    public void SetAuthToken(string? token)
    {
        _client.DefaultRequestHeaders.Authorization = token != null
            ? new AuthenticationHeaderValue("Bearer", token)
            : null;
    }

    // ── Auth ──

    public async Task<AuthResponse?> RegisterAsync(string fullName, string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { fullName, email, password });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
    }

    // ── Transactions ──

    public async Task<List<TransactionDto>> GetTransactionsAsync(string? search = null, string? categoryId = null)
    {
        var url = "/api/transactions";
        var queryParts = new List<string>();
        if (!string.IsNullOrEmpty(search)) queryParts.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrEmpty(categoryId)) queryParts.Add($"categoryId={categoryId}");
        if (queryParts.Count > 0) url += "?" + string.Join("&", queryParts);

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TransactionDto>>(JsonOptions) ?? new();
    }

    public async Task<TransactionDto?> CreateTransactionAsync(string description, decimal amount, string categoryId, DateTime? timestamp = null)
    {
        var response = await _client.PostAsJsonAsync("/api/transactions",
            new { description, amount, categoryId, timestamp = timestamp ?? DateTime.UtcNow });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TransactionDto>(JsonOptions);
    }

    public async Task<bool> DeleteTransactionAsync(string id)
    {
        var response = await _client.DeleteAsync($"/api/transactions/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<TransactionDto?> UpdateTransactionAsync(string id, string? description, decimal? amount, string? categoryId, DateTime? timestamp)
    {
        var response = await _client.PutAsJsonAsync($"/api/transactions/{id}",
            new { description, amount, categoryId, timestamp });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TransactionDto>(JsonOptions);
    }

    public async Task<List<WeeklySummaryDto>> GetWeeklySummaryAsync()
    {
        var response = await _client.GetAsync("/api/transactions/weekly-summary");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<WeeklySummaryDto>>(JsonOptions) ?? new();
    }

    public async Task<decimal> GetBalanceAsync()
    {
        var response = await _client.GetAsync("/api/transactions/balance");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BalanceResponse>(JsonOptions);
        return result?.Balance ?? 0;
    }

    public async Task<List<TransactionDto>> GetRecentTransactionsAsync(int count = 5)
    {
        var response = await _client.GetAsync($"/api/transactions/recent?count={count}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TransactionDto>>(JsonOptions) ?? new();
    }

    // ── Categories ──

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var response = await _client.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CategoryDto>>(JsonOptions) ?? new();
    }

    public async Task<CategoryDto?> CreateCategoryAsync(string name, decimal budgetLimit)
    {
        var response = await _client.PostAsJsonAsync("/api/categories",
            new { name, budgetLimit });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryDto>(JsonOptions);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(string id, string? name, decimal? budgetLimit)
    {
        var response = await _client.PutAsJsonAsync($"/api/categories/{id}",
            new { name, budgetLimit });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryDto>(JsonOptions);
    }

    public async Task<bool> DeleteCategoryAsync(string id)
    {
        var response = await _client.DeleteAsync($"/api/categories/{id}");
        return response.IsSuccessStatusCode;
    }

    // ── Analytics ──

    public async Task<List<CategorySpendingDto>> GetSpendingByCategoryAsync()
    {
        var response = await _client.GetAsync("/api/analytics/spending-by-category");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CategorySpendingDto>>(JsonOptions) ?? new();
    }

    public async Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync()
    {
        var response = await _client.GetAsync("/api/analytics/monthly-trend");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MonthlyTrendDto>>(JsonOptions) ?? new();
    }

    public async Task<decimal> GetTotalSpendingAsync()
    {
        var response = await _client.GetAsync("/api/analytics/total-spending");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TotalSpendingResponse>(JsonOptions);
        return result?.TotalSpending ?? 0;
    }

    // ── Profile ──

    public async Task<UserDto?> GetProfileAsync()
    {
        var response = await _client.GetAsync("/api/profile");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
    }

    public async Task<UserDto?> UpdateProfileAsync(string? fullName = null, string? currency = null, string? appearance = null)
    {
        var response = await _client.PutAsJsonAsync("/api/profile", new { fullName, currency, appearance });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
    }
}

// ── DTO Records ──

public record AuthResponse(string Token, UserDto User);
public record UserDto(string Id, string FullName, string Email, string? AvatarUrl, string Currency, string Appearance);
public record TransactionDto(string Id, string Description, decimal Amount, string CategoryId,
    string? CategoryName, string? CategoryIcon, DateTime Timestamp, string? Location);
public record CategoryDto(string Id, string Name, string IconName, decimal BudgetLimit, decimal CurrentSpending, int Percentage, string Color);
public record WeeklySummaryDto(string DayLabel, decimal Amount, bool IsHighlighted);
public record CategorySpendingDto(string Name, decimal Amount, int Percentage, string Color);
public record MonthlyTrendDto(string Month, decimal Amount, bool IsHighlighted);
public record BalanceResponse(decimal Balance);
public record TotalSpendingResponse(decimal TotalSpending);
