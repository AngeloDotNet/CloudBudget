using CloudBudget.API.DTOs;
using CloudBudget.API.Entities;
using CloudBudget.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IJwtTokenService jwt) : ControllerBase
{
    /// <summary>
    /// Login e ottenimento token JWT.
    /// </summary>
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

        var (token, expires) = await jwt.GenerateTokenAsync(user);

        var resp = new LoginResponseDto
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresAtUtc = expires
        };

        return Ok(resp);
    }

    /// <summary>
    /// Logout. Per JWT stateless non invalida il token lato server (se non si adotta revocation store).
    /// Questo endpoint esegue SignOut per autenticazioni cookie-based e fornisce suggerimento per l'invalidazione client-side.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // per cookie-based auth
        await signInManager.SignOutAsync();

        // Per JWT: informare client di eliminare il token. Per revoca lato server è necessario un meccanismo di revoca (not implemented here).
        return NoContent();
    }
}