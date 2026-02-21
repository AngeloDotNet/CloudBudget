using CloudBudget.API.Entities;

namespace CloudBudget.API.Services.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>
    /// Crea un nuovo refresh token persistente collegato allo userId e al jwtId (jti).
    /// </summary>
    Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string jwtId, CancellationToken ct = default);

    /// <summary>
    /// Valida e recupera un refresh token persistente dalla stringa token.
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Revoca (marca RevokedAt) il refresh token specificato.
    /// </summary>
    Task RevokeRefreshTokenAsync(string token, string? replacedBy = null, CancellationToken ct = default);

    /// <summary>
    /// Revoca tutti i refresh token attivi per un utente.
    /// </summary>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Revoca (persisti) un jti di JWT per negarne l'uso (revocation store).
    /// </summary>
    Task RevokeJwtAsync(string jti, string? reason = null, CancellationToken ct = default);
}