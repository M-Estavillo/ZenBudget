using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenBudget.Api.Data;
using ZenBudget.Api.Models.DTOs;

namespace ZenBudget.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ZenBudgetDbContext _db;

    public ProfileController(ZenBudgetDbContext db) => _db = db;

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _db.Users.FindAsync(GetUserId());
        if (user == null) return NotFound();

        return Ok(new UserDto(
            user.Id, user.FullName, user.Email,
            user.AvatarUrl, user.Currency, user.Appearance
        ));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(GetUserId());
        if (user == null) return NotFound();

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Currency != null) user.Currency = request.Currency;
        if (request.Appearance != null) user.Appearance = request.Appearance;

        await _db.SaveChangesAsync();

        return Ok(new UserDto(
            user.Id, user.FullName, user.Email,
            user.AvatarUrl, user.Currency, user.Appearance
        ));
    }
}
