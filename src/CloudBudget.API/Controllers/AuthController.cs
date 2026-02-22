using CloudBudget.API.DTOs;
using CloudBudget.API.Entities;
using CloudBudget.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IJwtTokenService jwt, IRefreshTokenService refreshService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto dto)
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

        // Extract client information from request
        var clientId = HttpContext.Request.Headers["X-Client-Id"].FirstOrDefault() ?? "web";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.FirstOrDefault();

        var refresh = await refreshService.CreateRefreshTokenAsync(user.Id, jti, clientId, ipAddress, userAgent);

        var resp = new LoginResponseDto
        {
            Token = token,
            ExpiresAtUtc = expires,
            RefreshToken = refresh.Token,
            RefreshTokenExpiresAtUtc = refresh.ExpiresAt
        };

        return Ok(resp);
    }

    /// <summary>
    /// Refresh token endpoint: scambia refresh token per nuovo JWT + nuovo refresh token (rotazione)
    /// </summary>
    [HttpPost("refresh")]
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

        // recupera user
        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user == null)
        {
            return Unauthorized();
        }

        // revoca il refresh token corrente e crea uno nuovo (rotation)
        await refreshService.RevokeRefreshTokenAsync(stored.Token, replacedBy: null);
        // anche revoca il jwt associato (per sicurezza)
        await refreshService.RevokeJwtAsync(stored.JwtId, "Rotated by refresh");

        // genera nuovo JWT + refresh
        var (newToken, newExpires, newJti) = await jwt.GenerateTokenAsync(user);

        // Extract client information from request
        var clientId = HttpContext.Request.Headers["X-Client-Id"].FirstOrDefault() ?? "web";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.FirstOrDefault();

        var newRefresh = await refreshService.CreateRefreshTokenAsync(user.Id, newJti, clientId, ipAddress, userAgent);

        var resp = new LoginResponseDto
        {
            Token = newToken,
            ExpiresAtUtc = newExpires,
            RefreshToken = newRefresh.Token,
            RefreshTokenExpiresAtUtc = newRefresh.ExpiresAt
        };

        // segna replacedBy per record precedente
        await refreshService.RevokeRefreshTokenAsync(stored.Token, replacedBy: newRefresh.Token);

        return Ok(resp);
    }

    /// <summary>
    /// Logout: revoca tutti i refresh token dell'utente e aggiunge jti del JWT corrente al revocation store
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        // ottieni user id dalla claim
        var uidClaim = User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(uidClaim, out var uid))
        {
            return Forbid();
        }

        // revoca tutti i refresh token dell'utente
        await refreshService.RevokeAllForUserAsync(uid);

        // dettaglio: revoca anche jti del JWT corrente (se presente)
        var jti = User.FindFirst("jti")?.Value ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

        if (!string.IsNullOrEmpty(jti))
        {
            await refreshService.RevokeJwtAsync(jti, "User logout");
        }

        // per cookie-based signout
        await signInManager.SignOutAsync();

        return NoContent();
    }
}