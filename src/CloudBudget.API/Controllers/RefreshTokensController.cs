using CloudBudget.API.Data;
using CloudBudget.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RefreshTokensController(CloudBudgetDbContext db, IRefreshTokenService refreshService) : ControllerBase
{

    // Admin: lista tutti i refresh token (opzionale filtro)
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var tokens = await db.RefreshTokens
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tokens);
    }

    // Admin: revoca uno specifico refresh token (by token string)
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "token is required" });
        }

        await refreshService.RevokeRefreshTokenAsync(token, replacedBy: null);
        // revoke associated jwt jti if any
        var stored = await refreshService.GetByTokenAsync(token);

        if (stored != null)
        {
            await refreshService.RevokeJwtAsync(stored.JwtId, "Revoked by admin");
        }

        return NoContent();
    }

    // User: lista i propri refresh token
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var uidClaim = User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(uidClaim, out var uid))
        {
            return Forbid();
        }

        var tokens = await db.RefreshTokens
            .AsNoTracking()
            .Where(t => t.UserId == uid)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tokens);
    }

    // User: revoca uno dei propri refresh token (by token string)
    [Authorize]
    [HttpPost("me/revoke")]
    public async Task<IActionResult> RevokeMine([FromQuery] string token)
    {
        var uidClaim = User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(uidClaim, out var uid))
        {
            return Forbid();
        }

        var stored = await refreshService.GetByTokenAsync(token);

        if (stored == null || stored.UserId != uid)
        {
            return NotFound();
        }

        await refreshService.RevokeRefreshTokenAsync(token, replacedBy: null);
        await refreshService.RevokeJwtAsync(stored.JwtId, "Revoked by owner");

        return NoContent();
    }
}