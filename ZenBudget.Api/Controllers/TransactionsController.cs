using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenBudget.Api.Models.DTOs;
using ZenBudget.Api.Services;

namespace ZenBudget.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
        => _transactionService = transactionService;

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] Guid? categoryId)
    {
        var transactions = await _transactionService.GetAllAsync(GetUserId(), search, categoryId);
        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var transaction = await _transactionService.GetByIdAsync(GetUserId(), id);
        return transaction == null ? NotFound() : Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { error = "Description is required." });

            var result = await _transactionService.CreateAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            var result = await _transactionService.UpdateAsync(GetUserId(), id, request);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _transactionService.DeleteAsync(GetUserId(), id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("weekly-summary")]
    public async Task<IActionResult> GetWeeklySummary()
    {
        var summary = await _transactionService.GetWeeklySummaryAsync(GetUserId());
        return Ok(summary);
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var balance = await _transactionService.GetBalanceAsync(GetUserId());
        return Ok(new { balance });
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 5)
    {
        var transactions = await _transactionService.GetRecentAsync(GetUserId(), count);
        return Ok(transactions);
    }
}
