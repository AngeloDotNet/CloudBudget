using CloudBudget.API.Entities;

namespace CloudBudget.API.Services.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Genera un token JWT per l'utente specificato. Restituisce la stringa token e la data di scadenza UTC.
    /// </summary>
    Task<(string Token, DateTime ExpiresAtUtc)> GenerateTokenAsync(ApplicationUser user);
}