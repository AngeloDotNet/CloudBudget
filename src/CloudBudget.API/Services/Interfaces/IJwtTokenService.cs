using CloudBudget.API.Entities;

namespace CloudBudget.API.Services.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Genera un token JWT per l'utente specificato. Restituisce la stringa token, la data di scadenza UTC e il jti generato.
    /// </summary>
    Task<(string Token, DateTime ExpiresAtUtc, string Jti)> GenerateTokenAsync(ApplicationUser user);
}