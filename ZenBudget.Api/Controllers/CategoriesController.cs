using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenBudget.Api.Models.DTOs;
using ZenBudget.Api.Services;

namespace ZenBudget.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
        => _categoryService = categoryService;

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync(GetUserId());
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(GetUserId(), id);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Category name is required." });

        if (request.BudgetLimit < 0)
            return BadRequest(new { error = "Budget limit cannot be negative." });

        var result = await _categoryService.CreateAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(GetUserId(), id, request);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _categoryService.DeleteAsync(GetUserId(), id);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
