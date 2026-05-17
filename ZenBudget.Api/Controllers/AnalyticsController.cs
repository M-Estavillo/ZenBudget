using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenBudget.Api.Services;

namespace ZenBudget.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsController(AnalyticsService analyticsService)
        => _analyticsService = analyticsService;

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("spending-by-category")]
    public async Task<IActionResult> GetSpendingByCategory()
    {
        var data = await _analyticsService.GetSpendingByCategoryAsync(GetUserId());
        return Ok(data);
    }

    [HttpGet("monthly-trend")]
    public async Task<IActionResult> GetMonthlyTrend([FromQuery] int months = 12)
    {
        var data = await _analyticsService.GetMonthlyTrendAsync(GetUserId(), months);
        return Ok(data);
    }

    [HttpGet("total-spending")]
    public async Task<IActionResult> GetTotalSpending()
    {
        var total = await _analyticsService.GetTotalSpendingThisMonthAsync(GetUserId());
        return Ok(new { totalSpending = total });
    }
}
