using CloudBudget.API.Data;
using CloudBudget.API.DTOs;
using CloudBudget.API.Entities;
using CloudBudget.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IJwtTokenService jwt,
    IRefreshTokenService refreshService, IGeoIpService geoIp, CloudBudgetDbContext dbContext) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Credenziali non valide." });
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Credenziali non valide." });
        }

        var (token, expires, jti) = await jwt.GenerateTokenAsync(user);

        // clientId: use provided, otherwise generate one for session (but better client generates)
        var clientId = string.IsNullOrEmpty(dto.ClientId) ? Guid.NewGuid().ToString() : dto.ClientId;

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = HttpContext.Request.Headers["User-Agent"].ToString();
        var country = HttpContext.Request.Headers["CF-IPCountry"].ToString(); // Cloudflare header, optional

        var geo = await geoIp.LookupAsync(ip ?? "", CancellationToken.None);
        var refresh = await refreshService.CreateRefreshTokenAsync(user.Id, jti, clientId, ip, ua, country);
        //var refresh = await refreshService.CreateRefreshTokenAsync(user.Id, jti, clientId, ip, ua, country);

        if (geo?.CountryCode != null)
        {
            refresh.Country = geo.CountryCode;
            dbContext.RefreshTokens.Update(refresh);
            await dbContext.SaveChangesAsync();
        }

        var resp = new LoginResponseDto
        {
            Token = token,
            ExpiresAtUtc = expires,
            RefreshToken = refresh.Token,
            RefreshTokenExpiresAtUtc = refresh.ExpiresAt
        };

        return Ok(resp);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("RefreshPolicy")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.RefreshToken))
        {
            return BadRequest();
        }

        var stored = await refreshService.GetByTokenAsync(dto.RefreshToken);
        if (stored == null)
        {
            return Unauthorized(new { message = "Refresh token non valido." });
        }

        if (!stored.IsActive)
        {
            return Unauthorized(new { message = "Refresh token scaduto o revocato." });
        }

        // ClientId check (sliding-window requires same client)
        if (!string.IsNullOrEmpty(dto.ClientId) && !string.Equals(dto.ClientId, stored.ClientId, StringComparison.Ordinal))
        {
            return Unauthorized(new { message = "ClientId mismatch." });
        }

        // recupera user
        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user == null)
        {
            return Unauthorized();
        }

        var clientId = dto.ClientId;
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Geo lookup
        var geo = await geoIp.LookupAsync(ip ?? "", CancellationToken.None);

        // Controllo: se token salvato ha Country e geo non corrisponde -> negare
        if (!string.IsNullOrEmpty(stored.Country) && !string.IsNullOrEmpty(geo?.CountryCode) && !string.Equals(stored.Country, geo.CountryCode, StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = "Location mismatch for refresh token. Verify or use same device." });
        }

        // revoke current (rotation) and associated jwt jti, generate new jwt + refresh
        await refreshService.RevokeRefreshTokenAsync(stored.Token, replacedBy: null);
        await refreshService.RevokeJwtAsync(stored.JwtId, "Rotated by refresh");

        var (newToken, newExpires, newJti) = await jwt.GenerateTokenAsync(user);
        var newRefresh = await refreshService.CreateRefreshTokenAsync(user.Id, newJti, stored.ClientId, stored.IpAddress, stored.UserAgent, stored.Country);

        // mark replacedBy for the previous token
        await refreshService.RevokeRefreshTokenAsync(stored.Token, replacedBy: newRefresh.Token);

        var resp = new LoginResponseDto
        {
            Token = newToken,
            ExpiresAtUtc = newExpires,
            RefreshToken = newRefresh.Token,
            RefreshTokenExpiresAtUtc = newRefresh.ExpiresAt
        };

        return Ok(resp);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var uidClaim = User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(uidClaim, out var uid))
        {
            return Forbid();
        }

        await refreshService.RevokeAllForUserAsync(uid);

        var jti = User.FindFirst("jti")?.Value ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
        if (!string.IsNullOrEmpty(jti))
        {
            await refreshService.RevokeJwtAsync(jti, "User logout");
        }

        await signInManager.SignOutAsync();

        return NoContent();
    }
}